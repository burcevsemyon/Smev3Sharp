using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smev3Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        [Obsolete("Используйте AddSmev3Client")]
        public static void UseSmev3Client(this IServiceCollection serviceCollection)
        {
            AddSmev3Client(serviceCollection);
        }

        public static void AddSmev3Client(this IServiceCollection serviceCollection, Func<SmevConfig> configure = null)
        {
            using var serviceProvider = serviceCollection.BuildServiceProvider();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var smevConfig = configure?.Invoke() ?? GetConfigFromAppConfig(configuration);

            var httpClientBuilder = serviceCollection.AddHttpClient("SmevClient", (httpClient) => httpClient.BaseAddress = smevConfig.Url);

            serviceCollection.AddSingleton<ISmev3ClientFactory>((sp) =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

                return new Smev3ClientFactory(httpClientFactory, smevConfig.ServiceConfigs);
            });
        }

        private static SmevConfig GetConfigFromAppConfig(IConfiguration config)
        {
            return new SmevConfig
            {
                Url = new Uri(config["Smev:Url"]),
                ServiceConfigs = config.GetSection("Smev:Services")
                                       .Get<Dictionary<string, SmevServiceConfig>>()
                                       .Select(i => new SmevServiceConfig
                                       {
                                           Mnemonic = i.Key,
                                           Container = i.Value.Container,
                                           Password = i.Value.Password,
                                           Thumbprint = i.Value.Thumbprint
                                       })
                                       .ToList()
            };
        }
    }
}
