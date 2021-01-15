using System;
using System.Net.Http;

namespace Smev3Client
{
    public interface ISmev3ClientContext
    {
        /// <summary>
        /// Клиент http
        /// </summary>
        HttpClient HttpClient { get; set; }

        /// <summary>
        /// Параметры сервиса СМЭВ
        /// </summary>
        SmevServiceConfig SmevServiceConfig { get; set; }
    }
}
