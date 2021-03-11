using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public async Task<Smev3ErrorInfo> ReadAsSmev3ErrorInfoAsync()
        {
            var xml = await HttpResponse.Content.ReadAsStringAsync();

            using var reader = new StringReader(xml);

            var serializer = new XmlSerializer(typeof(SoapEnvelope<SoapFault>));
            
            var soapFault = (SoapEnvelope<SoapFault>)serializer.Deserialize(reader);

            return soapFault?.Body == null ? null : new Smev3ErrorInfo(soapFault?.Body);
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
