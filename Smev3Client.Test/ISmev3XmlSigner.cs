using System.Xml;

namespace Smev3Client.Test
{
    public interface ISmev3XmlSigner
    {
        XmlElement SignXmlElement(XmlElement xml, string skid);
    }

}
