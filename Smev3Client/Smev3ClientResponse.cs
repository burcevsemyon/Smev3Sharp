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
        protected HttpResponseMessage _httpResponse;

        public Smev3ClientResponse(HttpResponseMessage response)
        {
            _httpResponse = response ?? throw new ArgumentNullException(nameof(response));
        }

        ~Smev3ClientResponse()
        {
            Dispose(false);
        }

        /// <summary>
        /// Открепляет HTTP ответ. Далее нельзя вызывать никакие методы объекта кроме Dispose
        /// </summary>
        /// <returns></returns>
        internal HttpResponseMessage DetachHttpResponse()
        {
            var response = _httpResponse;

            _httpResponse = null;

            return response;
        }

        /// <summary>
        /// Чтение элемента Body содержимого ответа как тип T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<T> ReadContentSoapBodyAsAsync<T>(CancellationToken cancellationToken = default)
            where T : ISoapEnvelopeBody, new()
        {
            ThrowIfDisposed();

            return _httpResponse.Content.ReadContentSoapBodyAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// Чтение ответа в строку
        /// </summary>
        /// <returns></returns>
        public Task<string> ReadAsStringAsync()
        {
            ThrowIfDisposed();

            return _httpResponse.Content.ReadAsStringAsync();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _)
        {
            _httpResponse?.Dispose();
            _httpResponse = null;
        }

        #endregion

        #region private

        private void ThrowIfDisposed()
        {
            if(_httpResponse == null)
            {
                throw new ObjectDisposedException(nameof(Smev3ClientResponse));
            }
        }

        #endregion
    }
}
