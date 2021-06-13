using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    public sealed class MessagePrimaryContentXml : IXmlSerializable
    {
        public XmlDocument Content { get; private set; }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            var content = new XmlDocument();

            content.Load(reader);

            Content = content;
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
