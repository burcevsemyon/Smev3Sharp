using System.Net.Http;

namespace Smev3Client
{

    public class Smev3ClientContext: ISmev3ClientContext
    {
        /// <summary>
        /// Фабрика клиреез клиента
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Конфигурация сервиса СМЭВ
        /// </summary>
        public SmevServiceConfig SmevServiceConfig { get; set; }
    }
}
