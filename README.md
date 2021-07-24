# Smev3Sharp

Частичная реализация HTTP клиента для СМЭВ 3 (версии схем 1.2) с поддержкой подписи XML средствами СКЗИ КРИПТО-ПРО для Linux

#### Реализованные методы:
1. SendRequest (Отправка запроса)
2. GetResponse (Получение ответа из очереди входящих ответов)
3. Ack (Подтверждение сообщения)

#### Зависимости:

.NET Standard 2.1  
System.Security.Cryptography.Xml 5.0.0  
System.Net.Http.Formatting.Extension 5.2.3  
Microsoft.Extensions.Http 5.0.0  
Microsoft.Extensions.DependencyInjection.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Binder 5.0.0  

* [Конфигурирование](#Конфигурирование-через-appsettingsjson)
* [Подключение](#Подключение)
* [Отправка запроса](#Отправка-запроса)
* [Получение ответа](#Получение-нетипизированного-ответа)
    * [Получение нетипизированного ответа](#Получение-нетипизированного-ответа)

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

#### Подключение:

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

            using ISmev3Client client = factory.Get("SMEV_SVC_MNEMONIC");

            ...
        }
    }
}
```

#### Отправка запроса:

```csharp
namespace Smev3ClientExample
{
    // дескриптор запроса сервиса
    public class SomeSmevServiceRequest
    {
        ...
    }

    class Program
    {
        static async void Main(string[] args)
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

            using ISmev3Client client = factory.Get("SMEV_SVC_MNEMONIC");

            // отправка запроса
            using Smev3ClientResponse response = await client.SendRequestAsync(
                                                                sendingContext, 
                                                                cancellationToken: default)
                                                              .ConfigureAwait(false);

            Console.WriteLine("Ид. сообщения СМЭВ: {0}", response.Data.MessageMetadata.MessageId);
        }
    }
}
```

#### Получение нетипизированного ответа:

Подобным образом целесообразно работать с ответами СМЭВ в случае если заранее неизвестно по каким типам сведений вернётся ответ сервиса из очереди

```csharp
...
using Smev3Client.Smev;
...

namespace Smev3ClientExample
{
    class Program
    {
        static async void Main(string[] args)
        {
            ...            
            
            using ISmev3Client client = factory.Get("SMEV_SVC_MNEMONIC");

            // получение ответа из очереди
            using Smev3ClientResponse response = await client.GetResponseAsync(
                                                                namespaceUri: null,
                                                                rootElementLocalName: null,
                                                                cancellationToken: default)
                                                              .ConfigureAwait(false);

            // полное содежимое в виде строки
            string responseRawContent = await response.ReadAsStringAsync()
                                                        .ConfigureAwait(false);

            Console.WriteLine("Полное содержимое входящего ответа СМЭВ: {0}", responseRawContent);

            // чтение метаданных и содержательной части ответа в Xml.
            // классы GetResponseResponse<T>, Response<T>, MessagePrimaryContentXml  описаны в пространстве имён Smev3Client.Smev
            // класс MessagePrimaryContentXml предназначен для чтения содержательной части ответа сервиса в XmlDocument
            GetResponseResponse<MessagePrimaryContentXml> smevMetaData = await response.ReadContentSoapBodyAsAsync<GetResponseResponse<MessagePrimaryContentXml>>
                                                                                        (cancellationToken: default)
                                                                                        .ConfigureAwait(false);

            Response<MessagePrimaryContentXml> responseContent = smevMetaData.ResponseMessage.Response;

            Console.WriteLine("Ответ ид. {0} на сообщение ид. {1}. Содержимое ответа: {3}",
                responseContent.MessageMetadata.MessageId,
                responseContent.OriginalMessageId,
                responseContent.SenderProvidedResponseData.MessagePrimaryContent.Content.Content.OuterXml);
        }
    }
}
```
