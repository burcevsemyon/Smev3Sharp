using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Smev3Client.Soap;
using Smev3Client.Xml;

namespace Smev3Client.Smev
{
    /// <summary>
    /// Возвращаемая структура метода "получить сообщение из моей входящей очереди, если очередь не пуста".
    /// </summary>
    public class GetResponseResponse<T> :
        ISoapEnvelopeBody,
        IXmlSerializable where T : new()
    {
        public ResponseMessage<T> ResponseMessage { get; private set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent(
                "Body", SoapConsts.SOAP_NAMESPACE, required: true,
                (bodyReader) =>
                {
                    bodyReader.ReadElementSubtreeContent(
                        "GetResponseResponse", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) =>
                        {
                            var responseMessage = new ResponseMessage<T>();
                            if (!r.IsEmptyElement)
                            {
                                responseMessage.ReadXml(reader);
                            }

                            ResponseMessage = responseMessage;
                        });
                });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
