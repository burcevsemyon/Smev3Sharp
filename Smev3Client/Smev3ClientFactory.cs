using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;

namespace Smev3Client
{
    internal class Smev3ClientFactory : ISmev3ClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Десткрипторы сервисов
        /// </summary>
        private readonly Dictionary<string, SmevServiceConfig> _serviceConfigs;

        public Smev3ClientFactory(
            IHttpClientFactory httpClientFactory,
            IDictionary<string, SmevServiceConfig> serviceConfigs)
        {
            _httpClientFactory = httpClientFactory ?? 
                throw new ArgumentNullException(nameof(httpClientFactory));

            if(serviceConfigs == null || serviceConfigs.Count == 0)
            {
                throw new ArgumentException("Не задано конфигураций ИС СМЭВ");
            }

            _serviceConfigs = serviceConfigs.ToDictionary(i => i.Key, i => new SmevServiceConfig(i.Value));
        }

        public ISmev3Client Get(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
            {
                throw new ArgumentException("Мнемоника сервиса не может быть пустой строкой");
            }

            if (!_serviceConfigs.ContainsKey(mnemonic))
            {
                throw new ArgumentException($"Сервис с мнемоникой {mnemonic} не зарегистрирован");
            }

            var httpClient = _httpClientFactory.CreateClient("smev");

            return new Smev3Client(new Smev3ClientContext
            {
                HttpClient = httpClient,
                SmevServiceConfig = new SmevServiceConfig(_serviceConfigs[mnemonic])
            });
        }
    }
}
