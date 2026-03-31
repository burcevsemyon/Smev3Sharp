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

        private readonly IReadOnlyDictionary<string, SmevServiceConfig> _serviceConfigsByMnemonic;

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

            var serviceConfigsByMnemonic = new Dictionary<string, SmevServiceConfig>(StringComparer.Ordinal);
            foreach (var serviceConfig in serviceConfigs)
            {
                if (serviceConfig?.Mnemonic == null || serviceConfigsByMnemonic.ContainsKey(serviceConfig.Mnemonic))
                {
                    continue;
                }

                serviceConfigsByMnemonic.Add(serviceConfig.Mnemonic, new SmevServiceConfig(serviceConfig));
            }

            _serviceConfigsByMnemonic = serviceConfigsByMnemonic;
        }

        public ISmev3Client Get(string mnemonic)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
            {
                throw new ArgumentException("Мнемоника сервиса не может быть пустой строкой");
            }

            return _clientsDic.GetOrAdd(mnemonic, (mmk) =>
            {
                if (!_serviceConfigsByMnemonic.TryGetValue(mmk, out var config))
                {
                    throw new ArgumentException($"Сервис с мнемоникой {mmk} не зарегистрирован");
                }

                var algorithm = new GostAsymmetricAlgorithm(config.Container, config.Password, config.Thumbprint);

                try
                {
                    return (client: new Smev3Client(_httpClientFactory.CreateClient("SmevClient"),
                                                        new Smev3XmlSigner(algorithm)),
                                                                              algorithm);
                }
                catch
                {
                    algorithm.Dispose();

                    throw;
                }
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
