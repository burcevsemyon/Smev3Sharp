using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Smev3Client.Smev;
using Smev3Client.Soap;

namespace Smev3Client.Tests
{
    [TestClass]
    public class Smev3ClientResponseTests
    {
        private sealed class NonSeekableStream : Stream
        {
            private readonly Stream _inner;

            public NonSeekableStream(Stream inner)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanWrite => _inner.CanWrite;
            public override bool CanSeek => false;
            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() => _inner.Flush();

            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        [TestMethod]
        public async Task ReadShortErrorInfo()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(File.ReadAllText("TestData/SoapFaultResponse.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var soapFault = await smevResponse.ReadSoapBodyAsAsync<SoapFault>().ConfigureAwait(false);

            Assert.IsNotNull(soapFault.FaultString);
        }

        [TestMethod]
        public async Task ReadDetailErrorInfo()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(File.ReadAllText("TestData/SoapFaultResponse_SignatureVerificationFault.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var soapFault = await smevResponse.ReadSoapBodyAsAsync<SoapFault>().ConfigureAwait(false);

            Assert.IsNotNull(soapFault.DetailXmlFragment);
        }

        [TestMethod]
        public async Task ReadSendRequestResponseMessageId_Exists()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponse.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadSoapBodyAsAsync<SendRequestResponse>().ConfigureAwait(false);

            Assert.IsNotNull(response.MessageMetadata.MessageId);
        }

        [TestMethod]
        public async Task ReadSendRequestResponseStatus_requestIsQueued()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponse.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadSoapBodyAsAsync<SendRequestResponse>().ConfigureAwait(false);

            Assert.AreEqual("requestIsQueued", response.MessageMetadata.Status);
        }

        [TestMethod]
        public async Task ReadGetResponseResponse_InvalidContent()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await File.ReadAllTextAsync("TestData/GetResponseResponse_InvalidContent.xml").ConfigureAwait(false))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>().ConfigureAwait(false);

            Assert.IsNotNull(response.ResponseMessage?.Response?.SenderProvidedResponseData?.ProcessingStatus?.Fault);
        }

        [TestMethod]
        public async Task ReadGetResponseResponse_ServiceResponseExists()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await File.ReadAllTextAsync("TestData/GetResponseResponse_ValidResponse.xml").ConfigureAwait(false))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>().ConfigureAwait(false);

            Assert.IsNotNull(response.ResponseMessage?.Response?.SenderProvidedResponseData?.MessagePrimaryContent?.Content);
        }

        [TestMethod]
        public async Task ReadGetResponseResponse_InvalidContent_ServiceResponseIsNull()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await File.ReadAllTextAsync("TestData/GetResponseResponse_InvalidContent.xml").ConfigureAwait(false))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>().ConfigureAwait(false);

            Assert.IsNull(response.ResponseMessage.Response.SenderProvidedResponseData.MessagePrimaryContent);
        }

        [TestMethod]
        public async Task ReadGetResponseResponse_EmptyQueue()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await File.ReadAllTextAsync("TestData/GetResponseResponse_EmptyQueue.xml").ConfigureAwait(false))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse
                .ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>()
                .ConfigureAwait(false);

            Assert.IsNull(response.ResponseMessage.Response);
        }

        [TestMethod]
        public async Task ReadGetResponseResponse_MultipartEmptyQueue()
        {
            var xmlBytes = await File.ReadAllBytesAsync("TestData/GetResponseResponse_MultipartEmptyQueue.xml").ConfigureAwait(false);

            var nonSeekable = new NonSeekableStream(new MemoryStream(xmlBytes));
            var content = new StreamContent(nonSeekable);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/mixed");
            content.Headers.ContentType.Parameters.Add(
                new System.Net.Http.Headers.NameValueHeaderValue("boundary", "f438a15e-9b5b-491f-9b47-aba4d00b8837"));

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse
                .ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>()
                .ConfigureAwait(false);

            Assert.IsNull(response.ResponseMessage.Response);
        }

        // RFC: multipart/* requires a boundary parameter, otherwise the payload cannot be reliably parsed.
        [TestMethod]
        public async Task ReadSoapBodyAsAsync_MultipartWithoutBoundary_ShouldThrow()
        {
            var soapFaultXml = await File.ReadAllTextAsync("TestData/SoapFaultResponse.xml").ConfigureAwait(false);

            var httpContent = new StringContent(soapFaultXml, Encoding.UTF8, "text/xml");
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/mixed"); // no boundary

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = httpContent };
            var smevResponse = new Smev3ClientResponse(httpResponse);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                smevResponse.ReadSoapBodyAsAsync<SoapFault>(CancellationToken.None)).ConfigureAwait(false);
        }
    }
}
