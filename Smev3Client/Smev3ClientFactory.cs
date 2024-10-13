using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Smev3Client.Crypt;

namespace Smev3Client
{
    internal class Smev3ClientFactory : ISmev3ClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ConcurrentDictionary<string, (Smev3Client client, GostAsymmetricAlgorithm algorithm)> _clientsDic =
                                    new ConcurrentDictionary<string, (Smev3Client client, GostAsymmetricAlgorithm algorithm)>();

        ~Smev3ClientFactory()
        {
            Dispose();
        }

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

            return _clientsDic.GetOrAdd(mnemonic, (mmk) =>
            {
                var config = _serviceConfigs.Find(i => i.Mnemonic == mmk)
                    ?? throw new ArgumentException($"Сервис с мнемоникой {mmk} не зарегистрирован");

                var algorithm = new GostAsymmetricAlgorithm(config.Container, config.Password, config.Thumbprint);

                return (client: new Smev3Client(_httpClientFactory.CreateClient("SmevClient"),
                                                                        new Smev3XmlSigner(algorithm)),
                                                                                              algorithm);
            })
            .client;
        }

        #region IDisposable

        public void Dispose()
        {
            foreach (var item in _clientsDic)
            {
                item.Value.algorithm.Dispose();
            }

            _clientsDic.Clear();

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
