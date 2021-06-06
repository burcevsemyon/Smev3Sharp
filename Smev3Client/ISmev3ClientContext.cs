using System.Net.Http;

namespace Smev3Client
{
    internal interface ISmev3ClientContext
    {
        /// <summary>
        /// Фабрика клиента http
        /// </summary>
        IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Параметры сервиса СМЭВ
        /// </summary>
        SmevServiceConfig SmevServiceConfig { get; }
    }
}
