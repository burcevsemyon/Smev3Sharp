using System;
using System.Net.Http;

namespace Smev3Client
{
    public class Smev3ClientResponse: IDisposable
    {
        /// <summary>
        /// Ответ СМЭВ
        /// </summary>
        public HttpResponseMessage HttpResponse { get; set; }

        public void Dispose()
        {
            HttpResponse?.Dispose();
        }
    }
}
