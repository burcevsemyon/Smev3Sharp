using System;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client
{
    public class MessagePrimaryContent : IXmlSerializable
    {
        public MessagePrimaryContent()
        {            
        }        

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MessagePrimaryContent2<T> : MessagePrimaryContent where T: new()
    {
        public MessagePrimaryContent2(){}

        public MessagePrimaryContent2(T content)
        {
            Content = content;
        }

        public T Content { get; set; }

        #region IXmlSerializable
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("MessagePrimaryContent", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);

            Smev3XmlSerializer.ToXmlElement(Content)
                .WriteTo(writer);

            writer.WriteEndElement();
        }

        #endregion
    }
}
