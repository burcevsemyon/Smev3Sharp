using System;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smev3Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void UseSmev3Client(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSmev3Client();
        }

        public static void AddSmev3Client(this IServiceCollection serviceCollection, Action<IHttpClientBuilder> httpClientBuilderConfigure = null)
        {
            var httpClientBuilder = serviceCollection.AddHttpClient("SmevClient", (serviceProvider, httpClient) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                httpClient.BaseAddress = new Uri(config["Smev:Url"]);
            });
            httpClientBuilderConfigure?.Invoke(httpClientBuilder);

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
