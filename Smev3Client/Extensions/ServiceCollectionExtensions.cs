using System;
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

        public static void AddSmev3Client(this IServiceCollection serviceCollection)
        {
            var httpClientBuilder = serviceCollection.AddHttpClient("SmevClient", (serviceProvider, httpClient) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                httpClient.BaseAddress = new Uri(config["Smev:Url"]);
            });

            serviceCollection.AddSingleton<ISmev3ClientFactory>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                var servicesConfigs = config.GetSection("Smev:Services")
                                                .Get<Dictionary<string, SmevServiceConfig>>();

                var httpClientFactory = serviceProvider
                                                .GetRequiredService<IHttpClientFactory>();

                return new Smev3ClientFactory(httpClientFactory, servicesConfigs);
            });
        }
    }
}
