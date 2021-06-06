using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Smev3Client.Xml;

namespace Smev3Client.Smev
{
    public class StatusParameter: IXmlSerializable
    {
        public string Key { get; set; }

        public string Value { get; set; }

        #region IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            //reader.ReadElementInnerContent(
            //    "ResponseMessage", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: true,
            //    (respReader) =>
            //    {
            //        var response = new Response();

            //        response.ReadXml(respReader);

            //        Response = response;

            //        // AttachmentContentList
            //        respReader.ReadElementContent(
            //            "AttachmentContentList", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
            //            (r) => r.Skip());

            //        // SMEVSignature
            //        respReader.ReadElementContent(
            //            "SMEVSignature", Smev3NameSpaces.MESSAGE_EXCHANGE_TYPES_1_2, required: false,
            //            (r) => r.Skip());
            //    });
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
