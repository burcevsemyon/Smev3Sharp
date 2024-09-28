using System.Xml;

namespace Smev3Client
{
    public interface ISmev3XmlSigner
    {
        /// <summary>
        /// Возвращает подпись xml елемента
        /// </summary>
        XmlElement SignXmlElement(XmlElement xml, string uri);
    }
}
