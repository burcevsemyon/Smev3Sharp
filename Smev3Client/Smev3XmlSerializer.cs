using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Smev3Client
{
    public static class Smev3XmlSerializer
    {
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            Indent = false,
            Encoding = new UTF8Encoding(false),
            OmitXmlDeclaration = true
        };

        public static XmlElement ToXmlElement<T>(T i) where T : new()
        {
            using var stream = new MemoryStream();

            using var writer = XmlWriter.Create(stream, XmlWriterSettings);

            SerializerCache<T>.Serializer.Serialize(writer, i);

            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            var doc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            doc.Load(stream);

            return doc.DocumentElement;
        }

        private static class SerializerCache<T>
            where T : new()
        {
            internal static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));
        }
    }
}
