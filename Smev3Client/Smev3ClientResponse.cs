using System;
using System.Net.Http;
using System.Threading.Tasks;

using Smev3Client.Http;
using Smev3Client.Soap;

namespace Smev3Client
{
    public class Smev3ClientResponse : IDisposable
    {
        public Smev3ClientResponse(HttpResponseMessage response)
        {            
            HttpResponse = response ?? throw new ArgumentNullException(nameof(response));            
        }

        ~Smev3ClientResponse()
        {
            Dispose(false);
        }

        /// <summary>
        /// Http ответ СМЭВ
        /// </summary>
        public HttpResponseMessage HttpResponse { get; private set; }

        /// <summary>
        /// Чтение элемента Body содержимого ответа как тип T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<T> ReadContentSoapBodyAsAsync<T>()
            where T : ISoapEnvelopeBody, new()
        {
            return HttpResponse.Content.ReadContentSoapBodyAsAsync<T>();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _)
        {
            HttpResponse?.Dispose();
            HttpResponse = null;
        }

        #endregion

        #region private


        #endregion
    }

    public class Smev3ClientResponse<T> : Smev3ClientResponse
        where T : ISoapEnvelopeBody, new()
    {
        /// <summary>
        /// Десериализованный объект
        /// </summary>
        public T Data { get; private set; }

        public Smev3ClientResponse(HttpResponseMessage response, T data) 
            : base(response)
        {
            Data = data;
        }
    }
}
