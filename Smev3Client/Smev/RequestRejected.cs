using System;
using System.Collections.Generic;
using System.Text;
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
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
