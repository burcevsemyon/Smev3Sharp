using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    public class SenderProvidedRequestData<T> :
        IXmlSerializable where T : new()
    {
        public SenderProvidedRequestData()
        {
        }

        public SenderProvidedRequestData(
            Guid messageId,
            string xmlElementId,
            MessagePrimaryContent<T> content)
        {
            MessageId = messageId;

            Content = content
                ?? throw new ArgumentNullException(nameof(content));

            Id = xmlElementId;
        }

        /// <summary>
        /// Атрибут Id xml элемента
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Ид. сообщения
        /// </summary>
        Guid MessageId { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        MessagePrimaryContent<T> Content { get; }

        public bool TestMessage { get; set; }

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
            writer.WriteStartElement("SenderProvidedRequestData", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2);

            writer.WriteAttributeString("Id", Id);

            writer.WriteElementString("MessageID", MessageId.ToString());

            Content.WriteXml(writer);

            if (TestMessage)
            {
                writer.WriteElementString("TestMessage", string.Empty);
            }

            writer.WriteEndElement();
        }

        #endregion
    }
}
