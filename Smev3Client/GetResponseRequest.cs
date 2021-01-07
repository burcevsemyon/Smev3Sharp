using Smev3Client.Soap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Smev3Client
{
    public class GetResponseRequest :
        ISoapEnvelopeBody,
        ISmev3Envelope        
    {
        #region members

        private readonly ISmev3XmlSigner _signer;

        private readonly MessageTypeSelector _requestData;

        private readonly SoapEnvelope<GetResponseRequest> _soapEnvelope;

        #endregion

        public GetResponseRequest()
        {
        }

        public GetResponseRequest(
            MessageTypeSelector requestData,
            ISmev3XmlSigner signer)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));

            _requestData = requestData ?? throw new ArgumentNullException(nameof(requestData));

            _soapEnvelope = new SoapEnvelope<GetResponseRequest>
            {
                Header = new SoapEnvelopeHeader
                {
                    Action = new SoapAction(nameof(Smev3Methods.GetResponse))
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
            writer.WriteStartElement("GetResponseRequest", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2);

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
