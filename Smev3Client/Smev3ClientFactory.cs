using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using Smev3Client.Crypt;

namespace Smev3Client
{
    internal class Smev3ClientFactory : ISmev3ClientFactory
    {
        private bool _disposed;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ConcurrentDictionary<
            string,
            (Smev3Client client, GostAsymmetricAlgorithm algorithm)
        > _clientsDic =
            new ConcurrentDictionary<
                string,
                (Smev3Client client, GostAsymmetricAlgorithm algorithm)
            >();

        private readonly Dictionary<string, SmevServiceConfig> _serviceConfigsByMnemonic;

        public Smev3ClientFactory(
            IHttpClientFactory httpClientFactory,
            List<SmevServiceConfig> serviceConfigs
        )
        {
            _httpClientFactory =
                httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            if (serviceConfigs.Count == 0)
            {
                throw new ArgumentException("Не задано конфигураций ИС СМЭВ");
            }

            var serviceConfigsByMnemonic = new Dictionary<string, SmevServiceConfig>(
                StringComparer.Ordinal
            );
            foreach (var serviceConfig in serviceConfigs)
            {
                if (serviceConfigsByMnemonic.ContainsKey(serviceConfig.Mnemonic))
                {
                    continue;
                }

                serviceConfigsByMnemonic.Add(
                    serviceConfig.Mnemonic,
                    new SmevServiceConfig(serviceConfig)
                );
            }

            _serviceConfigsByMnemonic = serviceConfigsByMnemonic;
        }

        public ISmev3Client Get(string mnemonic)
        {
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(mnemonic))
            {
                throw new ArgumentException("Мнемоника сервиса не может быть пустой строкой");
            }

            return _clientsDic
                .GetOrAdd(
                    mnemonic,
                    mmk =>
                    {
                        if (!_serviceConfigsByMnemonic.TryGetValue(mmk, out var config))
                        {
                            throw new ArgumentException(
                                $"Сервис с мнемоникой {mmk} не зарегистрирован"
                            );
                        }

                        var algorithm = new GostAsymmetricAlgorithm(
                            config.Container,
                            config.Password,
                            config.Thumbprint
                        );

                        try
                        {
                            return (
                                client: new Smev3Client(
                                    _httpClientFactory.CreateClient("SmevClient"),
                                    new Smev3XmlSigner(algorithm)
                                ),
                                algorithm
                            );
                        }
                        catch
                        {
                            algorithm.Dispose();

                            throw;
                        }
                    }
                )
                .client;
        }

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var item in _clientsDic)
            {
                item.Value.algorithm.Dispose();
            }

            _clientsDic.Clear();

            _disposed = true;
        }

        #endregion

        #region private

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Smev3ClientFactory));
            }
        }

        #endregion
    }
}
