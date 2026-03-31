using System.IO;
using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializersCache = new ConcurrentDictionary<Type, XmlSerializer>();
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings { Indent = false, Encoding = new UTF8Encoding(false) };

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

            using var writer = XmlWriter.Create(stream, XmlWriterSettings);

            var serializer = SerializersCache.GetOrAdd(GetType(), type => new XmlSerializer(type));

            serializer.Serialize(writer, this, SerializerNamespaces);

            writer.Flush();

            return stream.ToArray();
        }
    }
}
