using System;
using System.Xml;

namespace Smev3Client.Xml
{
    public static class XmlReaderExtensions
    {
        public static void ReadElementIfItCurrentOrRequired(
            this XmlReader reader, string localName, string @namespace, bool required, Action<XmlReader> action)
        {
            if (required || reader.IsStartElement(localName, @namespace))
            {
                action(reader);
            }
        }

        public static void ReadElementSubtreeContent(
            this XmlReader reader, string localName, string @namespace, bool required, Action<XmlReader> action)
        {
            if (required || reader.IsStartElement(localName, @namespace))
            {
                using (var subtreeReader = reader.ReadSubtree())
                {
                    subtreeReader.ReadStartElement(localName, @namespace);

                    action(subtreeReader);
                }

                if (!reader.IsEmptyElement)
                {
                    reader.ReadEndElement();
                }                
            }
        }
    }
}
