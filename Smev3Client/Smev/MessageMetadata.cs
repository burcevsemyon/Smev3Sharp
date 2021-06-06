using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Smev3Client.Xml;

namespace Smev3Client.Smev
{
    /// <summary>
    /// Маршрутная информация, заполняемая СМЭВ.
    /// </summary>
    public partial class MessageMetadata : IXmlSerializable
    {
        public Guid? MessageId { get; set; }

        public string MessageType { get; set; }

        public string Status { get; set; }        

        /// <summary>
        /// Дата и время отправки сообщения в СМЭВ.
        /// </summary>
        public DateTime SendingTimestamp { get; set; }

        /// <summary>
        /// Дата и время доставки сообщения, по часам СМЭВ.
        /// </summary>
        public DateTime? DeliveryTimestamp { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent(
                "MessageMetadata", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                (metaDataReader) =>
                {
                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "MessageId", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) => MessageId = Guid.Parse(r.ReadElementContentAsString()));

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "MessageType", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) => MessageType = r.ReadElementContentAsString());

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "Sender", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) => r.Skip());

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "SendingTimestamp", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) => SendingTimestamp = DateTime.Parse(r.ReadElementContentAsString()));

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "Recipient", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) => r.Skip());

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "DeliveryTimestamp", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) => DeliveryTimestamp = DateTime.Parse(r.ReadElementContentAsString()));

                    metaDataReader.ReadElementIfItCurrentOrRequired(
                        "Status", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) => Status = r.ReadElementContentAsString());
                });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
