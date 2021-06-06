using Smev3Client.Soap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Smev3Client.Smev
{
    public class AckRequest :
        ISoapEnvelopeBody,
        ISmev3Envelope
    {
        #region members

        private readonly ISmev3XmlSigner _signer;

        private readonly AckTargetMessage _requestData;

        private readonly SoapEnvelope<AckRequest> _soapEnvelope;

        #endregion

        public AckRequest()
        {
        }

        public AckRequest(
            AckTargetMessage requestData,
            ISmev3XmlSigner signer)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));

            _requestData = requestData ?? throw new ArgumentNullException(nameof(requestData));

            _soapEnvelope = new SoapEnvelope<AckRequest>
            {
                Header = new SoapEnvelopeHeader
                {
                    Action = new SoapAction(nameof(Smev3Methods.Ack))
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
            writer.WriteStartElement("AckRequest", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2);

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
