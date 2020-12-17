using System;

using Microsoft.Extensions.DependencyInjection;

namespace Smev3Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void UseSmev3Client(
            this IServiceCollection serviceCollection,
            Func<IServiceProvider, ISmev3ClientContext> contextFactory)
        {
            serviceCollection.AddSingleton<ISmev3Client>((serviceProvider) => new Smev3Client(contextFactory(serviceProvider)));
        }
    }
}
