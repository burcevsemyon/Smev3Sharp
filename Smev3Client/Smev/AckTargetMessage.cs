using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    ///<summary>
    /// Ссылка на сообщение, получение которого подтверждается методом Ack.
    /// Сюда нужно писать Id СМЭВ-сообщения, который берётся
    /// из //GetRequestResponse/.../SenderProvidedRequestData/MessageID/text() либо
    /// из //GetResponseResponse/.../SenderProvidedRequestData/MessageID/text().
    ///</summary>
    public class AckTargetMessage : IXmlSerializable
    {
        /// <summary>
        /// Ид. сообщения
        /// </summary>
        public Guid MessageID { get; set; }

        /// <summary>
        /// Ид. xml элемента
        /// </summary>
        public string Id { get; set; }

        public bool? Accepted { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("AckTargetMessage", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);

            writer.WriteAttributeString("Id", Id);

            if (Accepted.HasValue)
            {
                writer.WriteAttributeString("accepted", Accepted.Value.ToString().ToLower());
            }

            writer.WriteString(MessageID.ToString());

            writer.WriteEndElement();
        }
    }
}
