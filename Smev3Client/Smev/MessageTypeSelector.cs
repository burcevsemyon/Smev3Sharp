using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    /// <summary>
    //Селектор, с помощью которого при приёме запроса или ответа
    //                можно задать фильтр по типу запроса(ответа).
    //                Поскольку тип запроса или ответа однозначно определяется полным именем
    //                корневого XML-элемента его бизнес-данных,
    //                селектор представляет из себя структуру для задания этого имени.
    //                Если селектор пуст, это значит, что нужно принять запрос(ответ)
    //                без фильтрации по типам.
    /// </summary>
    public class MessageTypeSelector : IXmlSerializable
    {
        public MessageTypeSelector()
        {
        }

        public MessageTypeSelector(Uri namespaceURI, string rootElementLocalName)
        {
            if (!string.IsNullOrWhiteSpace(namespaceURI?.OriginalString) || !string.IsNullOrWhiteSpace(rootElementLocalName))
            {
                var paramName = string.IsNullOrWhiteSpace(namespaceURI?.OriginalString) ? nameof(namespaceURI)
                    : string.IsNullOrWhiteSpace(rootElementLocalName) ? nameof(rootElementLocalName) : null;

                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    throw new ArgumentException("Требуется указывать оба параметра. namespaceURI и rootElementLocalName", paramName);
                }
            }

            NamespaceURI = namespaceURI;
            RootElementLocalName = rootElementLocalName;
        }

        /// <summary>
        /// Текущая дата и время.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public string Id { get; set; }

        public Uri NamespaceURI { get; set; }

        public string RootElementLocalName { get; set; }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("MessageTypeSelector", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2);

            writer.WriteAttributeString("Id", Id);

            if (!string.IsNullOrWhiteSpace(NamespaceURI?.OriginalString))
            {
                writer.WriteElementString("NamespaceURI", NamespaceURI.OriginalString);
            }

            if (!string.IsNullOrWhiteSpace(RootElementLocalName))
            {
                writer.WriteElementString("RootElementLocalName", RootElementLocalName);
            }

            writer.WriteElementString("Timestamp", Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            writer.WriteEndElement();
        }
    }
}
