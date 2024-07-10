using System.Xml;
using System.Xml.Schema;
using Smev3Client.Xml;

namespace Smev3Client.Soap
{
    public class SoapFault : ISoapEnvelopeBody
    {
        public string FaultCode { get; set; }

        public string FaultString { get; set; }

        public string DetailXmlFragment { get; set; }

        public XmlSchema GetSchema()
        {
            throw new System.NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent("Body", SoapConsts.SOAP_NAMESPACE, required: true,
            (bodyReader) =>
            {
                bodyReader.ReadElementSubtreeContent("Fault", SoapConsts.SOAP_NAMESPACE, required: true,
                (faultReader) =>
                {
                    FaultCode = faultReader.ReadElementContentAsString("faultcode", string.Empty);
                    FaultString = faultReader.ReadElementContentAsString("faultstring", string.Empty);

                    faultReader.ReadElementIfItCurrentOrRequired("detail", string.Empty, required: false,
                    (detailReader) =>
                    {
                        DetailXmlFragment = detailReader.ReadOuterXml();
                    });
                });
            });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
