using System.Net;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;

using Smev3Client.Smev;
using Smev3Client.Soap;

namespace Smev3Client.Benchmarks
{
    [MemoryDiagnoser]
    public class SoapBenchmarks
    {
        private const string Boundary = "f438a15e-9b5b-491f-9b47-aba4d00b8837";

        private const string SoapFaultXml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "<soap:Body><soap:Fault><faultcode>soap:Server</faultcode>" +
            "<faultstring>Benchmark error</faultstring></soap:Fault></soap:Body></soap:Envelope>";

        private static readonly byte[] MultipartPayloadBytes = File.ReadAllBytes(
            Path.Combine(AppContext.BaseDirectory, "TestData", "GetResponseResponse_MultipartEmptyQueue.xml"));

        [Benchmark]
        public byte[] SerializeSoapEnvelope()
        {
            var envelope = new SoapEnvelope<BenchmarkBody>
            {
                Body = new BenchmarkBody("benchmark")
            };

            return envelope.Serialize();
        }

        [Benchmark]
        public async Task<SoapFault> DeserializeSoapBody_SinglePart()
        {
            using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SoapFaultXml, Encoding.UTF8, "text/xml")
            };

            using var response = new Smev3ClientResponse(httpResponse);
            return await response.ReadSoapBodyAsAsync<SoapFault>().ConfigureAwait(false);
        }

        [Benchmark]
        public async Task<GetResponseResponse<MultipartBenchmarkResponse>> DeserializeSoapBody_Multipart()
        {
            using var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = CreateMultipartContent()
            };

            using var response = new Smev3ClientResponse(httpResponse);
            return await response.ReadSoapBodyAsAsync<GetResponseResponse<MultipartBenchmarkResponse>>().ConfigureAwait(false);
        }

        private static HttpContent CreateMultipartContent()
        {
            var content = new StreamContent(new MemoryStream(MultipartPayloadBytes, writable: false));

            content.Headers.ContentType = new MediaTypeHeaderValue("multipart/mixed");
            content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Boundary));

            return content;
        }

        public sealed class BenchmarkBody : ISoapEnvelopeBody
        {
            public BenchmarkBody()
            {
            }

            public BenchmarkBody(string value)
            {
                Value = value;
            }

            public string Value { get; set; } = string.Empty;

            public XmlSchema GetSchema()
            {
                throw new System.NotImplementedException();
            }

            public void ReadXml(XmlReader reader)
            {
                Value = reader.ReadElementContentAsString("Value", string.Empty);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteElementString("Value", Value);
            }
        }

        [XmlRoot("MultipartBenchmarkResponse",
            Namespace = "urn://fake-smev-service-response",
            IsNullable = false)]
        public sealed class MultipartBenchmarkResponse
        {
            [XmlElement("Status")]
            public string Status { get; set; } = string.Empty;
        }
    }
}
