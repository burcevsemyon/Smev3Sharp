using System.Xml;
using System.Xml.Schema;

using Smev3Client.Smev;

namespace Smev3Client.Soap
{
    public class SoapFault: ISoapEnvelopeBody
    {
        public string FaultCode { get; set; }
        
        public string FaultString { get; set; }

        public string DetailCode { get; set; }

        public string DetailDescription { get; set; }

        public XmlSchema GetSchema()
        {
            throw new System.NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Body", SoapConsts.SOAP_NAMESPACE);
            reader.ReadStartElement("Fault", SoapConsts.SOAP_NAMESPACE);
            
            FaultCode = reader.ReadElementContentAsString("faultcode", string.Empty);
            FaultString = reader.ReadElementContentAsString("faultstring", string.Empty);

            if (reader.IsStartElement("detail"))
            {
                using var inner = reader.ReadSubtree();

                if (inner.Read())
                {
                    inner.ReadStartElement();

                    if (inner.NamespaceURI.Equals(Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_FAULTS_1_2))
                    {
                        inner.ReadStartElement();

                        DetailCode = inner.ReadElementContentAsString("Code", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);
                        DetailDescription = inner.ReadElementContentAsString("Description", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);                       
                    }
                }

                reader.Skip();
                reader.ReadEndElement();
            }
            
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
