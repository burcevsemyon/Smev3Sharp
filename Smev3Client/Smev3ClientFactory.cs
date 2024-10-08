﻿using System;
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
        private readonly List<SmevServiceConfig> _serviceConfigs;

        public Smev3ClientFactory(
            IHttpClientFactory httpClientFactory,
            List<SmevServiceConfig> serviceConfigs)
        {
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));

            if (serviceConfigs == null || serviceConfigs.Count == 0)
            {
                throw new ArgumentException("Не задано конфигураций ИС СМЭВ");
            }

            _serviceConfigs = serviceConfigs.ConvertAll(i => new SmevServiceConfig(i));
        }

        public ISmev3Client Get(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
            {
                throw new ArgumentException("Мнемоника сервиса не может быть пустой строкой");
            }

            if (!_serviceConfigs.Any(i => i.Mnemonic == mnemonic))
            {
                throw new ArgumentException($"Сервис с мнемоникой {mnemonic} не зарегистрирован");
            }

            return new Smev3Client(new Smev3ClientContext
            (
                _httpClientFactory,
                new SmevServiceConfig(_serviceConfigs.Find( i => i.Mnemonic == mnemonic))
            ));
        }
    }
}
