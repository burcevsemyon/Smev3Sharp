# Smev3Sharp

Частичная реализация HTTP клиента для СМЭВ 3 (версии схем 1.2) с поддержкой подписи XML средствами СКЗИ КРИПТО-ПРО для Linux

#### Реализованные методы:
1. SendRequest (Отправка запроса)
2. GetResponse (Получение ответа из очереди входящих ответов)
3. Ack (Подтверждение сообщения)

#### Зависимости:

.NET Standard 2.1  
System.Security.Cryptography.Xml 5.0.0  
Microsoft.Extensions.Http 5.0.0  
Microsoft.AspNetCore.WebUtilities 2.2.0  
Microsoft.Extensions.DependencyInjection.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Binder 5.0.0  
CryptoApiLiteSharp 1.1.0  

* [Конфигурирование](#Конфигурирование-через-appsettingsjson)
* [Подключение](#Подключение)
* [Отправка запроса](#Отправка-запроса)
* [Получение ответа](#Получение-нетипизированного-ответа)
    * [Получение нетипизированного ответа](#Получение-нетипизированного-ответа)
    * [Получение типизированного ответа](#Получение-типизированного-ответа)
* [Подтверждение сообщения](#Подтверждение-сообщения)
* [Обработка ошибок](#Обработка-ошибок)

#### Конфигурирование через appsettings.json:

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
                .AddSmev3Client();

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
        // поля
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
            using Smev3ClientResponse<SendRequestResponse> response = await client.SendRequestAsync(
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
            GetResponseResponse<MessagePrimaryContentXml> smevMetaData = await response.ReadSoapBodyAsAsync
                                                                                            <GetResponseResponse<MessagePrimaryContentXml>>
                                                                                            (cancellationToken: default)
                                                                                        .ConfigureAwait(false);

            Response<MessagePrimaryContentXml> responseMetadata = smevMetaData.ResponseMessage.Response;

            Console.WriteLine("Ответ ид. {0} на сообщение ид. {1}. Содержимое ответа сервиса: {3}",
                responseMetadata.MessageMetadata.MessageId,
                responseMetadata.OriginalMessageId,
                responseMetadata.SenderProvidedResponseData.MessagePrimaryContent.Content.Content.OuterXml);
        }
    }
}
```

#### Получение типизированного ответа:

В случае если используется выборка ответов с фильтрацией по типам сведений, то её можно совместить с десериализацией содержательной части ответа сервиса

```csharp
...
using Smev3Client.Smev;
...

namespace Smev3ClientExample
{
    // дескриптор ответа сервиса
    public class SomeSmevServiceRespose
    {
        // поля
        ...
    }

    class Program
    {
        static async void Main(string[] args)
        {
            ...            
            
            using ISmev3Client client = factory.Get("SMEV_SVC_MNEMONIC");

            // получение ответа из очереди
            using Smev3ClientResponse<GetResponseResponse<SomeSmevServiceRespose>> response = await client.GetResponseAsync<SomeSmevServiceRespose>(
                                                    namespaceUri: new Uri("urn://some-smev-service-namespace"),
                                                    rootElementLocalName: "SomeSmevServiceRespose",
                                                    cancellationToken: default)
                                              .ConfigureAwait(false);

            Response<SomeSmevServiceRespose> responseMetadata = response.Data.ResponseMessage.Response;

            Console.WriteLine("Ответ ид. {0} на сообщение ид. {1}.",
                responseMetadata.MessageMetadata.MessageId,
                responseMetadata.OriginalMessageId);

            // содержательная часть ответа сервиса
            SomeSmevServiceRespose serviceResponse = responseMetadata.SenderProvidedResponseData.MessagePrimaryContent.Content;
        }
    }
}
```

#### Подтверждение сообщения:

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
            using Smev3ClientResponse<GetResponseResponse<MessagePrimaryContentXml>> response = await client.GetResponseAsync<MessagePrimaryContentXml>(
                                                    namespaceUri: null,
                                                    rootElementLocalName: null,
                                                    cancellationToken: default)
                                              .ConfigureAwait(false);

            // подтверждение сообщения
            using var ackResponse = await client.AckAsync(
                                            response.Data.ResponseMessage.Response.MessageMetadata.MessageId.Value,
                                            cancellationToken: default)
                                            .ConfigureAwait(false);
        }
    }
}
```

#### Обработка ошибок:

В случае если СМЭВ возвращает ошибку через SOAP FAULT, методы клиента бросают исключение типа Smev3Exception

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

            try
            {
                // подтверждение сообщения
                using var ackResponse = await client.AckAsync(...)
                                                .ConfigureAwait(false);
            }
            catch (Smev3Exception ex)
            {
                // обработка ошибки СМЭВ
                ...
            }
        }
    }
}
```
