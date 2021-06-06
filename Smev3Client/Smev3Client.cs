using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
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

        public async Task<Smev3ClientResponse<SendRequestResponse>> SendRequestAsync<T>(SendRequestExecutionContext<T> context, 
                                                                      CancellationToken cancellationToken)
            where T : new()
        {
            ThrowExceptionIfDisposedFlagSetted();

            var httpResponse = await SendAsync(
                new SendRequestRequest<T>
                (
                    requestData: new SenderProvidedRequestData<T>(
                        messageId: Rfc4122.GenerateUUIDv1(),
                        xmlElementId: "SIGNED_BY_CONSUMER",
                        content: new MessagePrimaryContent<T>(context.RequestData)
                        )
                    { TestMessage = context.IsTest },
                    signer: new Smev3XmlSigner(_algorithm)
                ),
                cancellationToken
            );

            var soapEnvelopeBody = await httpResponse
                                            .Content
                                            .ReadContentSoapBodyAsAsync<SendRequestResponse>();

            return new Smev3ClientResponse<SendRequestResponse>(httpResponse, soapEnvelopeBody);
        }

        /// <summary>
        /// Получение ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse> GetResponseAsync(CancellationToken cancellationToken)
        {
            ThrowExceptionIfDisposedFlagSetted();

            var httpResponse = await SendAsync(
                new GetResponseRequest(
                    requestData: new MessageTypeSelector
                    {
                        Timestamp = DateTime.Now,
                        Id = "SIGNED_BY_CONSUMER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                cancellationToken);

            return new Smev3ClientResponse(httpResponse);
        }

        /// <summary>
        /// Подтверждение получения ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Smev3ClientResponse> AckAsync(Guid messageId, CancellationToken cancellationToken)
        {
            ThrowExceptionIfDisposedFlagSetted();

            var httpResponse = await SendAsync(
                new AckRequest(
                    new AckTargetMessage
                    {
                        MessageID = messageId,
                        Id = "SIGNED_BY_CALLER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                cancellationToken);

            return new Smev3ClientResponse(httpResponse);
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
        /// <param name="requestData"></param>
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

            cancellationToken.ThrowIfCancellationRequested();

            using var httpClient = _context.HttpClientFactory.CreateClient("smev");

            var httpResponse = await httpClient.PostAsync(
                string.Empty,
                content,
                cancellationToken);            

            if (httpResponse.StatusCode == HttpStatusCode.InternalServerError)
            {
                var faultInfo = await httpResponse.Content.ReadContentSoapBodyAsAsync<SoapFault>();

                throw new Smev3ClientException($"FaultCode: {faultInfo.FaultCode}. FaultString: {faultInfo.FaultString}.");
            }

            return httpResponse;
        }

        /// <summary>
        /// Бросает исключение если объект утилизирован
        /// </summary>
        private void ThrowExceptionIfDisposedFlagSetted()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Smev3Client));
            }
        }

        #endregion
    }
}
