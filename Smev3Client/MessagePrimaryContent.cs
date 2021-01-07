using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client
{
    public class MessagePrimaryContent<T> : 
        IXmlSerializable where T: new()
    {
        public MessagePrimaryContent(){}

        public MessagePrimaryContent(T content)
        {
            Content = content;
        }

        public T Content { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("MessagePrimaryContent", 
                Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);

            Smev3XmlSerializer.ToXmlElement(Content)
                .WriteTo(writer);

            writer.WriteEndElement();
        }

        #endregion
    }
}
