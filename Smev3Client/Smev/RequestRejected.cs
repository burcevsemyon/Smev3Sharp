using Smev3Client.Xml;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    public class RequestRejected : IXmlSerializable
    {
        /// <summary>
        /// Код причины отклонения запроса.
        /// </summary>
        public string RejectionReasonCode { get; set; }

        /// <summary>
        /// Причина отклонения запроса, в человекочитаемом виде.
        /// </summary>
        public string RejectionReasonDescription { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent(nameof(RequestRejected), Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                rejectionReader =>
                {
                    rejectionReader.ReadElementIfItCurrentOrRequired("RejectionReasonCode", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        r => RejectionReasonCode = r.ReadElementContentAsString());
                    rejectionReader.ReadElementIfItCurrentOrRequired("RejectionReasonDescription", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        r => RejectionReasonDescription = r.ReadElementContentAsString());
                });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
