using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Smev3Client.Smev
{
    /// <summary>
    /// Возвращаемое значение метода "Послать запрос": запрос принят.
    /// Если запрос не может быть принят, информация о причине отказа передаётся через SOAP fault, см.WSDL-описание сервиса.
    /// </summary>
    public class SendRequestResponse: IXmlSerializable
    {
        /// <summary>
        /// Данные о сообщении: ID, присвоенный СМЭВ, дата приёма по часам СМЭВ, результат маршрутизации, etc.
        /// </summary>
        public MessageMetadata MessageMetadata { get; set; }

        #region IXmlSerializable

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
            throw new NotImplementedException();
        }

        #endregion
    }
}
