using System;
using System.IO;
using System.Collections.Concurrent;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Smev3Client
{
    public static class Smev3XmlSerializer
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializersCache = new ConcurrentDictionary<Type, XmlSerializer>();

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

            var serializer = SerializersCache.GetOrAdd(typeof(T), type => new XmlSerializer(type));

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
