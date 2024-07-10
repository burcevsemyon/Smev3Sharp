using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Soap
{
    public class SoapAction : IXmlSerializable
    {
        private readonly string _actionName;

        public SoapAction()
        {
        }

        public SoapAction(string actionName)
        {
            _actionName = actionName ??
                throw new ArgumentNullException(nameof(actionName));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("mustUnderstand", "1");

            writer.WriteString("urn:" + _actionName);
        }
    }
}
