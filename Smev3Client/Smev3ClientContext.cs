using System;
using System.Net.Http;

namespace Smev3Client
{

    public class Smev3ClientContext: ISmev3ClientContext
    {
        public IHttpClientFactory HttpClientFactory { get; set; }

        public SmevServiceConfig SmevServiceConfig { get; set; }        
    }
}
