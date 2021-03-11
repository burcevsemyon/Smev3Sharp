using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client
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
        /// <summary>
        /// Текущая дата и время.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public string Id { get; set; }

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

            writer.WriteElementString("Timestamp", Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            writer.WriteEndElement();
        }
    }
}
