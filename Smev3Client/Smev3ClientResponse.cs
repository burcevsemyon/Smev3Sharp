using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Smev3Client.Http;
using Smev3Client.Soap;

namespace Smev3Client
{
    public class Smev3ClientResponse : IDisposable
    {
        private bool _disposed;

        protected HttpResponseMessage _httpResponse;

        public Smev3ClientResponse(HttpResponseMessage response)
        {
            _httpResponse = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Открепляет HTTP ответ. Далее нельзя вызывать никакие методы объекта кроме Dispose
        /// </summary>
        internal HttpResponseMessage DetachHttpResponse()
        {
            ThrowIfDisposed();

            var response = _httpResponse;

            _httpResponse = null;

            return response;
        }

        /// <summary>
        /// Чтение элемента Body содержимого ответа как тип T
        /// </summary>
        public Task<T> ReadSoapBodyAsAsync<T>(CancellationToken cancellationToken = default)
            where T : ISoapEnvelopeBody, new()
        {
            ThrowIfDisposed();

            return _httpResponse.Content.ReadSoapBodyAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// Чтение ответа в строку
        /// </summary>
        public Task<string> ReadSoapBodyAsStringAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            return _httpResponse.Content.ReadSoapBodyAsStringAsync(cancellationToken);
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _httpResponse?.Dispose();
            _httpResponse = null;
            _disposed = true;
        }

        #endregion

        #region private

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Smev3ClientResponse));
            }
        }

        #endregion
    }
}
