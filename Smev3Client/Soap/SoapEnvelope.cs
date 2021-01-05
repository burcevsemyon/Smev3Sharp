using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Smev3Client.Soap
{
    [XmlRoot(
     Namespace = SoapConsts.SOAP_NAMESPACE,
     ElementName = "Envelope",
     IsNullable = false)]
    public class SoapEnvelope<TBody> 
        where TBody : ISoapEnvelopeBody, new()
    {
        public XmlSerializerNamespaces SerializerNamespaces { get; } = new XmlSerializerNamespaces();

        public SoapEnvelope()
        {
            SerializerNamespaces.Add("s", SoapConsts.SOAP_NAMESPACE);
        }

        /// <summary>
        /// Заголовок
        /// </summary>
        [XmlElement(ElementName = "Header")]
        public SoapEnvelopeHeader Header { get; set; }

        /// <summary>
        /// Тело
        /// </summary>
        [XmlElement(ElementName = "Body")]
        public TBody Body { get; set; }

        public byte[] Serialize()
        {
            using var stream = new MemoryStream();

            using var writer = XmlWriter.Create(stream,
                new XmlWriterSettings { Indent = false, Encoding = new UTF8Encoding(false) }); 

            var serializer = new XmlSerializer(GetType());

            serializer.Serialize(writer, this, SerializerNamespaces);

            writer.Flush();

            return stream.ToArray();
        }
    }
}
