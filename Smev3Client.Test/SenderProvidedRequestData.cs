using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Test
{
    public class SenderProvidedRequestData: IXmlSerializable
    {
        public SenderProvidedRequestData(Guid messageId)
        {
            MessageId = messageId;
        }

        /// <summary>
        /// Ид. сообщения
        /// </summary>
        Guid MessageId { get; }

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
