# Smev3Sharp

Частичная реализация HTTP клиента для СМЭВ3 (версии схем 1.2) с поддержкой подписи xml средствами СКЗИ КРИПТО-ПРО для Linux

**Реализованные методы:**
1. SendRequest
2. GetResponse
3. Ack

**Зависимости:**

.NET Standard 2.1  
System.Security.Cryptography.Xml 5.0.0  
Microsoft.Extensions.Http 5.0.0  
Microsoft.Extensions.DependencyInjection.Abstractions 5.0.0  
Microsoft.Extensions.Configuration.Abstractions 5.0.0  
Microsoft.AspNet.WebApi.Client 5.2.7

**Конфигурирование через appsettings.json:**

```
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
