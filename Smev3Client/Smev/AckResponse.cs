using System;
using System.Xml;
using System.Xml.Schema;

using Smev3Client.Soap;
using Smev3Client.Xml;

namespace Smev3Client.Smev
{

    public class AckResponse: ISoapEnvelopeBody
    {
        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent(
                "Body", SoapConsts.SOAP_NAMESPACE, required: true,
                (bodyReader) =>
                {
                    bodyReader.ReadElementSubtreeContent(
                        "AckResponse", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) =>
                        {
                        });
                });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
