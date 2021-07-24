using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;

using Smev3Client.Crypt;
using Smev3Client.Utils;
using Smev3Client.Smev;
using Smev3Client.Soap;
using Smev3Client.Http;

namespace Smev3Client
{
    internal class Smev3Client : IDisposable, ISmev3Client
    {
        #region members

        /// <summary>
        /// Параметры клиента
        /// </summary>
        private ISmev3ClientContext _context;

        /// <summary>
        /// Криптоалгоритм
        /// </summary>
        private GostAsymmetricAlgorithm _algorithm;

        /// <summary>
        /// Флаг утилизированого объекта
        /// </summary>
        private bool _disposed = false;

        #endregion        

        public Smev3Client(ISmev3ClientContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));

            _algorithm = new GostAsymmetricAlgorithm(
                                context.SmevServiceConfig.Container,
                                context.SmevServiceConfig.Password,
                                context.SmevServiceConfig.Thumbprint);
        }

        ~Smev3Client()
        {
            Dispose(false);
        }

        /// <summary>
        /// Отправка запроса
        /// </summary>
        /// <typeparam name="TServiceRequest"></typeparam>
        /// <param name="context">Параметры методы</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse<SendRequestResponse>> SendRequestAsync<TServiceRequest>(SendRequestExecutionContext<TServiceRequest> context,
                                                                      CancellationToken cancellationToken)
            where TServiceRequest : new()
        {
            ThrowIfDisposed();

            HttpResponseMessage httpResponse = null;
            try
            {
                httpResponse = await SendAsync(
                    new SendRequestRequest<TServiceRequest>
                    (
                        requestData: new SenderProvidedRequestData<TServiceRequest>(
                            messageId: Rfc4122.GenerateUUIDv1(),
                            xmlElementId: "SIGNED_BY_CONSUMER",
                            content: new MessagePrimaryContent<TServiceRequest>(context.RequestData)
                            )
                        { TestMessage = context.IsTest },
                        signer: new Smev3XmlSigner(_algorithm)
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);

                var soapEnvelopeBody = await httpResponse
                                                .Content
                                                .ReadSoapBodyAsAsync<SendRequestResponse>(cancellationToken)
                                                .ConfigureAwait(false);

                return new Smev3ClientResponse<SendRequestResponse>(httpResponse, soapEnvelopeBody);
            }
            catch
            {
                httpResponse?.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Получение сообщения из очереди входящих ответов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="namespaceUri"></param>
        /// <param name="rootElementLocalName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse> GetResponseAsync(Uri namespaceUri, string rootElementLocalName,
                                                    CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var httpResponse = await SendAsync(
                new GetResponseRequest(
                    requestData: new MessageTypeSelector(namespaceUri, rootElementLocalName)
                    {
                        Timestamp = DateTime.Now,
                        Id = "SIGNED_BY_CONSUMER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                        cancellationToken
                 )
                 .ConfigureAwait(false);

            return new Smev3ClientResponse(httpResponse);
        }

        /// <summary>
        /// Получение сообщения из очереди входящих ответов c десереализацией ответа в тип T
        /// </summary>
        /// <typeparam name="TServiceResponse"></typeparam>
        /// <param name="namespaceUri"></param>
        /// <param name="rootElementLocalName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse<GetResponseResponse<TServiceResponse>>> GetResponseAsync<TServiceResponse>(Uri namespaceUri, string rootElementLocalName,
                                                CancellationToken cancellationToken)
            where TServiceResponse : new()
        {
            using var response = await GetResponseAsync(namespaceUri, rootElementLocalName, cancellationToken)
                                        .ConfigureAwait(false);

            var data = await response.ReadSoapBodyAsAsync<GetResponseResponse<TServiceResponse>>()
                                        .ConfigureAwait(false);

            return new Smev3ClientResponse<GetResponseResponse<TServiceResponse>>(response.DetachHttpResponse(), data);
        }

        /// <summary>
        /// Подтверждение получения ответа
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse<AckResponse>> AckAsync(Guid messageId, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            var httpResponse = await SendAsync(
                new AckRequest(
                    new AckTargetMessage
                    {
                        MessageID = messageId,
                        Id = "SIGNED_BY_CALLER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                cancellationToken)
                .ConfigureAwait(false);

            var data = await httpResponse.Content.ReadSoapBodyAsAsync<AckResponse>(cancellationToken)
                                        .ConfigureAwait(false);

            return new Smev3ClientResponse<AckResponse>(httpResponse, data);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _)
        {
            _algorithm?.Dispose();
            _algorithm = null;
            _context = null;
            _disposed = true;
        }

        #endregion

        #region private

        /// <summary>
        /// Отправка конверта
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="cancellationToken"></param>
        private async Task<HttpResponseMessage> SendAsync(ISmev3Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            var envelopeBytes = envelope.Get();

            var content = new ByteArrayContent(
                envelopeBytes, 0, envelopeBytes.Length);

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Soap)
            {
                CharSet = "utf-8"
            };

            HttpResponseMessage httpResponse = null;
            try
            {
                using var httpClient = _context.HttpClientFactory.CreateClient("SmevClient");

                httpResponse = await httpClient.PostAsync(string.Empty, content, cancellationToken)
                                               .ConfigureAwait(false);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    return httpResponse;
                }

                var faultInfo = await httpResponse.Content.ReadSoapBodyAsAsync<SoapFault>(cancellationToken)
                                                  .ConfigureAwait(false);

                throw new Smev3Exception(
                    $"FaultCode: {faultInfo.FaultCode}. FaultString: {faultInfo.FaultString}.")
                {
                    FaultInfo = faultInfo
                };
            }
            catch
            {
                httpResponse?.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Бросает исключение если объект утилизирован
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Smev3Client));
            }
        }

        #endregion
    }
}
