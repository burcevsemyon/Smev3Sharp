using System.Xml;

namespace Smev3Client
{
    public interface ISmev3XmlSigner
    {
        XmlElement SignXmlElement(XmlElement xml, string skid);
    }

}
