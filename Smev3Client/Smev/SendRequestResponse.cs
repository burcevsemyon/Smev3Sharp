using System;
using System.Xml;
using System.Xml.Schema;

using Smev3Client.Soap;

namespace Smev3Client.Smev
{
    /// <summary>
    /// Возвращаемое значение метода "Послать запрос": запрос принят.
    /// Если запрос не может быть принят, информация о причине отказа передаётся через SOAP fault, см.WSDL-описание сервиса.
    /// </summary>
    public partial class SendRequestResponse: ISoapEnvelopeBody
    {
        /// <summary>
        /// Данные о сообщении: ID, присвоенный СМЭВ, дата приёма по часам СМЭВ, результат маршрутизации, etc.
        /// </summary>
        public MessageMetadata MessageMetadata { get; } = new MessageMetadata();

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Body", SoapConsts.SOAP_NAMESPACE);
            reader.ReadStartElement("SendRequestResponse", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2);

            MessageMetadata.ReadXml(reader);

            // SMEVSignature
            reader.Skip();

            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
