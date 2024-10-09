using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;

using Smev3Client.Utils;
using Smev3Client.Smev;
using Smev3Client.Soap;
using Smev3Client.Http;

namespace Smev3Client
{
    internal class Smev3Client : IDisposable, ISmev3Client
    {
        #region members

        private readonly HttpClient _httpClient;

        private readonly ISmev3XmlSigner _signer;

        #endregion

        public Smev3Client(HttpClient httpClient, ISmev3XmlSigner signer)
        {
            _httpClient = httpClient ??
                throw new ArgumentNullException(nameof(httpClient));

            _signer = signer ??
                throw new ArgumentNullException(nameof(signer));
        }

        /// <summary>
        /// Отправка запроса
        /// </summary>
        /// <typeparam name="TServiceRequest">Тип запроса</typeparam>
        /// <param name="context">Параметры метода</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task<Smev3ClientResponse<SendRequestResponse>> SendRequestAsync<TServiceRequest>(SendRequestExecutionContext<TServiceRequest> context,
                                                                      CancellationToken cancellationToken)
            where TServiceRequest : new()
        {
            HttpResponseMessage httpResponse = null;
            try
            {
                var envelope = new SendRequestRequest<TServiceRequest>
                    (
                        requestData: new SenderProvidedRequestData<TServiceRequest>(
                            messageId: Rfc4122.GenerateUUIDv1(),
                            xmlElementId: "SIGNED_BY_CONSUMER",
                            content: new MessagePrimaryContent<TServiceRequest>(context.RequestData)
                            )
                        { TestMessage = context.IsTest },
                        signer: _signer
                    );

                var envelopeBytes = envelope.Get();

                context.OnBeforeSend?.Invoke(envelopeBytes);

                httpResponse = await SendAsync(envelopeBytes, cancellationToken)
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
        public async Task<Smev3ClientResponse> GetResponseAsync(Uri namespaceUri, string rootElementLocalName,
                                                    CancellationToken cancellationToken)
        {
            var envelope = new GetResponseRequest(
                    requestData: new MessageTypeSelector(namespaceUri, rootElementLocalName)
                    {
                        Timestamp = DateTime.Now,
                        Id = "SIGNED_BY_CONSUMER"
                    },
                    signer: _signer);

            var envelopeBytes = envelope.Get();

            var httpResponse = await SendAsync(envelopeBytes, cancellationToken)
                                        .ConfigureAwait(false);

            return new Smev3ClientResponse(httpResponse);
        }

        /// <summary>
        /// Получение сообщения из очереди входящих ответов c десереализацией ответа в тип T
        /// </summary>
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
        public async Task<Smev3ClientResponse<AckResponse>> AckAsync(Guid messageId, CancellationToken cancellationToken)
        {
            var envelope = new AckRequest(
                    new AckTargetMessage
                    {
                        MessageID = messageId,
                        Id = "SIGNED_BY_CALLER"
                    },
                    signer: _signer);

            var envelopeBytes = envelope.Get();

            var httpResponse = await SendAsync(envelopeBytes, cancellationToken)
                                        .ConfigureAwait(false);

            var data = await httpResponse.Content.ReadSoapBodyAsAsync<AckResponse>(cancellationToken)
                                        .ConfigureAwait(false);

            return new Smev3ClientResponse<AckResponse>(httpResponse, data);
        }

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region private

        /// <summary>
        /// Отправка конверта
        /// </summary>
        private async Task<HttpResponseMessage> SendAsync(byte[] envelopeBytes, CancellationToken cancellationToken)
        {
            if (envelopeBytes == null)
            {
                throw new ArgumentNullException(nameof(envelopeBytes));
            }

            var content = new ByteArrayContent(
                envelopeBytes, 0, envelopeBytes.Length);

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Soap)
            {
                CharSet = "utf-8"
            };

            HttpResponseMessage httpResponse = null;
            try
            {
                httpResponse = await _httpClient.PostAsync(string.Empty, content, cancellationToken)
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

        #endregion
    }
}
