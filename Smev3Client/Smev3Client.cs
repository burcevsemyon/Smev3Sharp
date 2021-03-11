using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;

using Smev3Client.Crypt;
using Smev3Client.Utils;

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
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Smev3ClientResponse> SendRequestAsync<T>(SendRequestExecutionContext<T> context, 
                                                                CancellationToken cancellationToken) where T : new()
        {
            ThrowExceptionIfDisposedFlagSetted();

            return SendAsync(
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
        }

        /// <summary>
        /// Получение ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Smev3ClientResponse> GetResponseAsync(CancellationToken cancellationToken)
        {
            ThrowExceptionIfDisposedFlagSetted();

            return SendAsync(
                new GetResponseRequest(
                    requestData : new MessageTypeSelector
                    {
                        Timestamp = DateTime.Now,
                        Id = "SIGNED_BY_CONSUMER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                cancellationToken);
        }

        /// <summary>
        /// Подтверждение получения ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Smev3ClientResponse> AckAsync(Guid messageId, CancellationToken cancellationToken)
        {
            ThrowExceptionIfDisposedFlagSetted();

            return SendAsync(
                new AckRequest(
                    new AckTargetMessage
                    {
                        MessageID = messageId,
                        Id = "SIGNED_BY_CALLER"
                    },
                    signer: new Smev3XmlSigner(_algorithm)),
                cancellationToken);
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
        private async Task<Smev3ClientResponse> SendAsync(ISmev3Envelope envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            var envelopeBytes = envelope.Get();

            var content = new ByteArrayContent(
                envelopeBytes, 0, envelopeBytes.Length);

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Soap)
            {
                CharSet = "utf-8"
            };

            cancellationToken.ThrowIfCancellationRequested();

            using var httpClient = _context.HttpClientFactory.CreateClient("smev");

            var response = await httpClient.PostAsync(
                string.Empty,
                content,
                cancellationToken);

            return new Smev3ClientResponse(response);
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
