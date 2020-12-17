
using Microsoft.Extensions.DependencyInjection;
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
        static readonly IServiceCollection _serviceCollection = new ServiceCollection();

        static async Task Main(string[] args)
        {
            

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
