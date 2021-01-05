
using Microsoft.Extensions.DependencyInjection;
using Smev3Client.Crypt;
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
        //static readonly IServiceCollection _serviceCollection = new ServiceCollection();

        /*
         * 					<ESIARegisterCertificateRequest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn://mincomsvyaz/esia/reg_service/register_certificate/1.4.2">
						<RoutingCode>PROD</RoutingCode>
						<serialNumber>016FF958008DAC3F9042F0E907257D76F0</serialNumber>
						<issuerOrgName>ООО "АСТРАЛ-М"</issuerOrgName>
						<startDate>10.12.2020</startDate>
						<expiryDate>10.12.2021</expiryDate>
						<ownerType>IB</ownerType>
						<snils>078-439-227 02</snils>
						<personINN>166003726000</personINN>
						<lastName>Антонов</lastName>
						<firstName>Владимир</firstName>
						<middleName>Вячеславович</middleName>
						<gender>M</gender>
						<birthDate>12.12.1970</birthDate>
						<birthPlace>гор. Казань</birthPlace>
						<doc>
							<type xmlns="urn://mincomsvyaz/esia/commons/rg_sevices_types/1.4.2">RF_PASSPORT</type>
							<series xmlns="urn://mincomsvyaz/esia/commons/rg_sevices_types/1.4.2">9216</series>
							<number xmlns="urn://mincomsvyaz/esia/commons/rg_sevices_types/1.4.2">013277</number>
							<issueId xmlns="urn://mincomsvyaz/esia/commons/rg_sevices_types/1.4.2">160008</issueId>
							<issueDate xmlns="urn://mincomsvyaz/esia/commons/rg_sevices_types/1.4.2">25.12.2015</issueDate>
						</doc>
						<citizenship>RUS</citizenship>
						<ogrn>319169000093278</ogrn>
         */
        static async Task Main(string[] args)
        {
            var regCertData = new ESIARegisterCertificateRequestType();

            regCertData.RoutingCode = EnvType.TESIA;
            regCertData.serialNumber = "016FF958008DAC3F9042F0E907257D76F0";
            regCertData.issuerOrgName = "ООО \"АСТРАЛ - М\"";
            regCertData.startDate = "10.12.2020";
            regCertData.expiryDate = "10.12.2020";
            regCertData.ownerType = ownertypeType.IB;
            regCertData.snils = "078-439-227 02";
            regCertData.personINN = "166003726000";
            regCertData.lastName = "Антонов";
            regCertData.firstName = "Владимир";
            regCertData.middleName = "Вячеславович";
            regCertData.gender = genderType.M;
            regCertData.genderSpecified = true;
            regCertData.birthDate = "12.12.1970";
            regCertData.birthPlace = "гор. Казань";
            regCertData.citizenship = "RUS";
            regCertData.ogrn = "319169000093278";
            regCertData.doc = new document3Type();
            regCertData.doc.type = documenttypeType.RF_PASSPORT;
            regCertData.doc.series = "9216";
            regCertData.doc.number = "013277";
            regCertData.doc.issueId = "160008";
            regCertData.doc.issueDate = "25.12.2015";

            var client = new Smev3Client(new Smev3ClientContext(new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws")));

            await client.SendAsync(new SendRequestRequest
            (
                new SenderProvidedRequestData(
                    Rfc4122.GenerateUUIDv1(),
                    "SIGNED_BY_CONSUMER",
                    new MessagePrimaryContent2<ESIARegisterCertificateRequestType>(regCertData)
                    )
                { TestMessage = false },
                new Smev3XmlSigner(
                    //new GostAsymmetricAlgorithm("1.pfx", "1", "b6ef0efe3a9d4f940d902c0e1c5401afc642c454")
                    //new GostAsymmetricAlgorithm("1.pfx", "1", "fe3770f0e51861692b4bff4b6f629881d29a5ba3")
                    new GostAsymmetricAlgorithm("ka.pfx", "1", "26f898eeac34bb2723290e7670e2a27d90e9b496")

                    )
            ), default);
            
            //Smev3XmlSigner signer = new Smev3XmlSigner();
            //var doc = new XmlDocument();

            //doc.Load(@"C:\temp\1.xml");

            //var sign = signer.SignXmlElement(doc.DocumentElement, "f5f4d5b6bc852c94031313f89b862cf6a10b180f");

            //return;

            //var client = new Smev3Client(new Smev3ClientContext(new Uri("http://smev3-n0.test.gosuslugi.ru:7500/smev/v1.2/ws")));

            //await client.SendAsync(new SendRequestRequest
            //(
            //    new SenderProvidedRequestData(Rfc4122.GenerateUUIDv1()),
            //    new Smev3XmlSigner()
            //), default);



            // SerializeObject("c:/temp/1.xml");

            //Smev3EnvelopeHelper baseEnvelope = new Smev3EnvelopeHelper();

            //var doc =  baseEnvelope.BuildSoapMsg("SendRequest", null);

            // var dd = doc.ToString();

            Console.WriteLine("Hello World!");
        }

        //public static void SerializeObject(string filename)
        //{
        //    var soapEnvelope = new SoapEnvelope();

        //    soapEnvelope.Header = new SoapEnvelopeHeader();
        //    soapEnvelope.Header.Action = new SoapAction("SendRequest");

        //    // A TextWriter is needed to write the file.
        //    TextWriter writer = new StreamWriter(filename);

        //    var nss = new XmlSerializerNamespaces();
        //    nss.Add("soap", SoapConsts.SOAP_NAMESPACE);

        //    // Create the XmlSerializer using the XmlAttributeOverrides.
        //    XmlSerializer s =
        //    new XmlSerializer(typeof(SoapEnvelope));

        //    // Serialize the object and close the TextWriter.
        //    s.Serialize(writer, soapEnvelope, nss);
        //    writer.Close();
        //}
    }
}
