using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Smev3Client
{
    public class Smev3XmlSerializer
    {
        public static XmlElement ToXmlElement<T>(T i) where T: new()
        {
            using var stream = new MemoryStream();

            using var writer = XmlWriter.Create(stream, 
                new XmlWriterSettings { 
                    Indent = false,
                    Encoding = new UTF8Encoding(false),
                    OmitXmlDeclaration = true });

            var serializer = new XmlSerializer(typeof(T));

            serializer.Serialize(writer, i);

            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var doc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            doc.Load(stream);

            return doc.DocumentElement;
        }
    }
}
