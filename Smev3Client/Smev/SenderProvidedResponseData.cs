using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Smev3Client.Xml;

namespace Smev3Client.Smev
{
    public class SenderProvidedResponseData<T> :
        IXmlSerializable where T : new()
    {
        public string Id { get; set; }

        public Guid MessageID { get; set; }

        public RequestRejected[] RequestRejectionResons { get; set; }

        public RequestStatus Status { get; set; }

        public AsyncProcessingStatus ProcessingStatus { get; set; }

        /// <summary>
        /// Содержательная часть ответа, XML-документ.
        /// </summary>
        public MessagePrimaryContent<T> MessagePrimaryContent { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadElementSubtreeContent(
                "SenderProvidedResponseData", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                (respReader) =>
                {
                    respReader.ReadElementIfItCurrentOrRequired(
                        "MessageID", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) => MessageID = Guid.Parse(r.ReadElementContentAsString()));

                    respReader.ReadElementIfItCurrentOrRequired(
                        "To", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
                        (r) => r.Skip());

                    respReader.ReadElementIfItCurrentOrRequired(
                        "MessagePrimaryContent", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_BASIC_1_2, required: false,
                        (r) =>
                        {
                            var msgPrimaryContent = new MessagePrimaryContent<T>();

                            msgPrimaryContent.ReadXml(r);

                            MessagePrimaryContent = msgPrimaryContent;
                        });

                    respReader.ReadElementIfItCurrentOrRequired(
                        "PersonalSignature", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) =>
                        {
                            r.Skip();
                        });

                    respReader.ReadElementIfItCurrentOrRequired(
                        "AttachmentHeaderList", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) =>
                        {
                            r.Skip();
                        });

                    respReader.ReadElementIfItCurrentOrRequired(
                        "RefAttachmentHeaderList", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) =>
                        {
                            r.Skip();
                        });

                    respReader.ReadElementIfItCurrentOrRequired(
                        "AsyncProcessingStatus", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
                        (r) =>
                        {
                            var status = new AsyncProcessingStatus();

                            status.ReadXml(r);

                            ProcessingStatus = status;
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
