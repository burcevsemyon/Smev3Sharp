using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Smev3Client.Crypt;
using Smev3Client.Extensions;
using Smev3Client.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Smev3Client.Test
{
    class Program
    {
        static public void Main(string[] args)
        {
            var builder = new HostBuilder()
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.UseSmev3Client();
                        })
                        .ConfigureHostConfiguration(configHost => {

                            configHost.AddJsonFile("appsettings.json", optional: false);

                        })
                        .UseConsoleLifetime();

            var host = builder.Build();

            var factory = host.Services.GetRequiredService<ISmev3ClientFactory>();

            var client = factory.Get("SVC_MNEMONIC");

            
        }

        //static public IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureHostConfiguration(configHost => {                    

        //            configHost.AddJsonFile("appsettings.json", optional: false);

        //        })
        //        .ConfigureServices((hostContext, services) => {
                    
        //            services.UseSmev3Client();

        //        });
                

        //static readonly IServiceCollection _serviceCollection = new ServiceCollection();

        //static IServiceProvider ServiceProvider { get; set; }

        //static async Task Main(string[] args)
        //{


        //    _serviceCollection.UseSmev3Client();

        //    ServiceProvider = _serviceCollection.BuildServiceProvider();

        //    var factory = ServiceProvider.GetRequiredService<ISmev3ClientFactory>();

        //    Console.WriteLine("Hello World!");
        //}
    }
}


//using Microsoft.Extensions.DependencyInjection;
//using Smev3Client.Crypt;
//using Smev3Client.Utils;
//using System;
//using System.Buffers;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Linq;
//using System.Xml.Serialization;

//namespace Smev3Client.Test
//{
//    class Program
//    {
//        //static readonly IServiceCollection _serviceCollection = new ServiceCollection();


//        static async Task Main(string[] args)
//        {
//            var regCertData = new ESIARegisterCertificateRequestType();

//            regCertData.RoutingCode = EnvType.TESIA;
//            regCertData.serialNumber = "016FF958008DAC3F9042F0E907257D76F0";
//            regCertData.issuerOrgName = "ООО \"АСТРАЛ - М\"";
//            regCertData.startDate = "10.12.2020";
//            regCertData.expiryDate = "10.12.2020";
//            regCertData.ownerType = ownertypeType.IB;
//            regCertData.snils = "078-439-227 02";
//            regCertData.personINN = "166003726000";
//            regCertData.lastName = "Антонов";
//            regCertData.firstName = "Владимир";
//            regCertData.middleName = "Вячеславович";
//            regCertData.gender = genderType.M;
//            regCertData.genderSpecified = true;
//            regCertData.birthDate = "12.12.1970";
//            regCertData.birthPlace = "гор. Казань";
//            regCertData.citizenship = "RUS";
//            regCertData.ogrn = "319169000093278";
//            regCertData.doc = new document3Type();
//            regCertData.doc.type = documenttypeType.RF_PASSPORT;
//            regCertData.doc.series = "9216";
//            regCertData.doc.number = "013277";
//            regCertData.doc.issueId = "160008";
//            regCertData.doc.issueDate = "25.12.2015";

//            var client = new Smev3Client(new Smev3ClientContext(new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws")));

//            await client.SendAsync(new SendRequestRequest<ESIARegisterCertificateRequestType>
//            (
//                new SenderProvidedRequestData<ESIARegisterCertificateRequestType>(
//                    Rfc4122.GenerateUUIDv1(),
//                    "SIGNED_BY_CONSUMER",
//                    new MessagePrimaryContent<ESIARegisterCertificateRequestType>(regCertData)
//                    )
//                { TestMessage = false },
//                new Smev3XmlSigner(
//                    new GostAsymmetricAlgorithm("ka.pfx", "1", "26f898eeac34bb2723290e7670e2a27d90e9b496")

//                    )
//            ), default);

//            await client.SendAsync(
//                new GetResponseRequest(
//                    new MessageTypeSelector 
//                    { 
//                        Timestamp = DateTime.Now, 
//                        Id = "SIGNED_BY_CONSUMER" 
//                    },
//                    new Smev3XmlSigner(
//                        new GostAsymmetricAlgorithm("ka.pfx", "1", "26f898eeac34bb2723290e7670e2a27d90e9b496"))
//                    ), 
//                default);

//            await client.SendAsync(
//                new AckRequest(
//                    new AckTargetMessage
//                    {
//                        MessageID = Rfc4122.GenerateUUIDv1(),
//                        Id = "SIGNED_BY_CALLER"
//                    },
//                    new Smev3XmlSigner(
//                        new GostAsymmetricAlgorithm("ka.pfx", "1", "26f898eeac34bb2723290e7670e2a27d90e9b496"))
//                    ),
//                default);



//            Console.WriteLine("Hello World!");
//        }
//    }
//}
