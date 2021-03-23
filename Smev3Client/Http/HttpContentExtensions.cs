using System.Xml;
using System.Xml.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

using Smev3Client.Soap;

namespace Smev3Client.Http
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadContentSoapBodyAsAsync<T>(this HttpContent httpContent) 
            where T: ISoapEnvelopeBody, new()
        {
            using var stream = await httpContent.ReadAsStreamAsync();

            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true
            });

            var serializer = new XmlSerializer(typeof(SoapEnvelope<T>));

            var envelope = (SoapEnvelope<T>)serializer.Deserialize(reader);

            return envelope.Body;
        }
    }
}
