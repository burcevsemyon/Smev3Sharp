using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Test.Soap
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
            SerializerNamespaces.Add("soap", SoapConsts.SOAP_NAMESPACE);
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

            using var writer = new StreamWriter(stream, Encoding.UTF8); 

            var serializer = new XmlSerializer(GetType());

            serializer.Serialize(writer, this, SerializerNamespaces);

            writer.Flush();

            return stream.ToArray();
        }
    }
}
