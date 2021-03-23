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

        /// <summary>
        /// Признак ответа с ошибкой
        /// </summary>
        public bool IsErrorResponse => HttpResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError;

        /// <summary>
        /// Http ответ СМЭВ
        /// </summary>
        public HttpResponseMessage HttpResponse { get; private set; }

        /// <summary>
        /// Чтение объекта с причиной ошибки, передаваемой через SOAP fault
        /// </summary>
        /// <returns></returns>
        public async Task<Smev3ErrorInfo> ReadAsSmev3ErrorInfoAsync()
        {
            var soapFault = await HttpResponse.Content
                .ReadContentSoapBodyAsAsync<SoapFault>();

            return new Smev3ErrorInfo(soapFault);
        }

        #region IDisposable

        public void Dispose()
        {
            HttpResponse?.Dispose();
            HttpResponse = null;
        }

        #endregion

        #region private


        #endregion
    }
}
