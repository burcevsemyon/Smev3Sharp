using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    public class RequestStatus : IXmlSerializable
    {
        /// <summary>
        /// Код бизнес статуса запроса.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Бизнес статус запроса, в человекочитаемом виде.
        /// </summary>
        public string StatusDescription { get; set; }

        public StatusParameter[] StatusParameters { get; set; }

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
