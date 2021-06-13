using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Smev3Client.Xml;

namespace Smev3Client.Smev
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
            reader.ReadElementSubtreeContent(
                "MessagePrimaryContent", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2, required: true,
                (contentReader) =>
                {
                    if(typeof(T) == typeof(MessagePrimaryContentXml))
                    {
                        var content = new T();

                        ((IXmlSerializable)content).ReadXml(contentReader);

                        Content = content;
                    }
                    else
                    {
                        var serializer = new XmlSerializer(typeof(T));

                        Content = (T)serializer.Deserialize(contentReader);
                    }
                });
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
