using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    /// <summary>
    /// Маршрутная информация, заполняемая СМЭВ.
    /// </summary>
    public class MessageMetadata : IXmlSerializable
    {
        public Guid MessageId { get; set; }

        public string MessageType { get; set; }

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
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
