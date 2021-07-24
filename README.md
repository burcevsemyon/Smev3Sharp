# Smev3Sharp

Частичная реализация HTTP клиента для СМЭВ 3 (версии схем 1.2) с поддержкой подписи XML средствами СКЗИ КРИПТО-ПРО для Linux

**Реализованные методы:**
1. SendRequest (Отправка запроса)
2. GetResponse (Получение ответа из очереди входящих ответов)
3. Ack (Подтверждение сообщения)

**Зависимости:**

.NET Standard 2.1  
System.Security.Cryptography.Xml 5.0.0  
System.Net.Http.Formatting.Extension 5.2.3  
Microsoft.Extensions.Http 5.0.0  
Microsoft.Extensions.DependencyInjection.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Binder 5.0.0  

* [Конфигурирование](#Конфигурирование-через-appsettingsjson)
* [Подключение](#Подключение)

##### Конфигурирование через appsettings.json:

```json
{
  "Smev": {
    "Url": "http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws",
    "Services": {
      "SMEV_SVC_MNEMONIC_1": {
        "Container": "path_to_crypto_pro_pfx_1.pfx",
        "Password": "password_to_pfx_1",
        "Thumbprint": "thumbprint_of_cert"
      },
      "SMEV_SVC_MNEMONIC_2": {
        "Container": "path_to_crypto_pro_pfx_2.pfx",
        "Password": "password_to_pfx_2",
        "Thumbprint": "thumbprint_of_cert"
      }
    }
  }
}
```

##### Подключение:

```csharp
using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Smev3Client;
using Smev3Client.Extensions;

namespace Smev3ClientExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();

            configBuilder.AddJsonFile("appsettings.json", optional: false);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton<IConfiguration>(configBuilder.Build())
                .UseSmev3Client();

            var services = serviceCollection.BuildServiceProvider();

            var factory = services.GetRequiredService<ISmev3ClientFactory>();

            using var client = factory.Get("SMEV_SVC_MNEMONIC");

            ...
        }
    }
}
```

**Отправка запроса:**

```csharp
namespace Smev3ClientExample
{
    // параметры запроса сервиса
    public class SomeSmevServiceRequest
    {
        ...
    }

    class Program
    {
        static void Main(string[] args)
        {
            ...            
            
            var sendingContext = new SendRequestExecutionContext<SomeSmevServiceRequest>
            {
                IsTest = true,
                RequestData = new SomeSmevServiceRequest
                {
                    // инициализация полей
                    ...
                }
            };

            using var client = factory.Get("SMEV_SVC_MNEMONIC");

            // отправка запроса
            using var response = await client.SendRequestAsync(sendingContext, cancellationToken: default)
                                             .ConfigureAwait(false);

            Console.WriteLine("Ид. сообщения СМЭВ: {0}", response.Data.MessageMetadata.MessageId);
        }
    }
}
```
