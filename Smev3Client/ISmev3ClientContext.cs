using System;
using System.Net.Http;

namespace Smev3Client
{
    public interface ISmev3ClientContext
    {
        /// <summary>
        /// Фабрика клиента http
        /// </summary>
        IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Параметры сервиса СМЭВ
        /// </summary>
        SmevServiceConfig SmevServiceConfig { get; set; }
    }
}
