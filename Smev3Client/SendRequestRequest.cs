using System;
using System.Xml;
using System.Xml.Schema;

using Smev3Client.Soap;

namespace Smev3Client
{
    /// <summary>
    /// Тело метода SendRequest
    /// </summary>
    public class SendRequestRequest :
        ISoapEnvelopeBody,
        ISmev3Envelope
    {
        #region members

        private ISmev3XmlSigner _signer;

        private SenderProvidedRequestData _requestData;

        private SoapEnvelope<SendRequestRequest> _soapEnvelope;        

        #endregion

        public SendRequestRequest()
        {
        }

        public SendRequestRequest(
            SenderProvidedRequestData requestData,
            ISmev3XmlSigner signer)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _requestData = requestData ?? throw new ArgumentNullException(nameof(requestData));

            _soapEnvelope = new SoapEnvelope<SendRequestRequest>
            {
                Header = new SoapEnvelopeHeader
                {
                    Action = new SoapAction("SendRequest")
                },
                Body = this
            };
        }

        #region ISmev3Envelope

        public byte[] Get()
        {
            return _soapEnvelope.Serialize();
        }

        #endregion

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("SendRequestRequest", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2);
            
            _requestData.WriteXml(writer);

            writer.WriteStartElement("CallerInformationSystemSignature");

            _signer.SignXmlElement(
                    Smev3XmlSerializer.ToXmlElement(_requestData),
                    _requestData.Id)
                .WriteTo(writer);

            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        
        #endregion
    }
}
