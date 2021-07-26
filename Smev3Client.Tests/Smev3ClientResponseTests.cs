using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Smev3Client.Smev;
using Smev3Client.Soap;

namespace Smev3Client.Tests
{
    [TestClass]
    public class Smev3ClientResponseTests
    {
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
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponce.xml"))
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
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponce.xml"))
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
                Content = new StringContent(File.ReadAllText("TestData/GetResponseResponce_InvalidContent.xml"))
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
                Content = new StringContent(File.ReadAllText("TestData/GetResponseResponse_ValidResponse.xml"))
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
                Content = new StringContent(File.ReadAllText("TestData/GetResponseResponce_InvalidContent.xml"))
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
                Content = new StringContent(File.ReadAllText("TestData/GetResponseResponce_EmptyQueue.xml"))
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
            var content = new StringContent(File.ReadAllText("TestData/GetResponseResponce_MultipartEmptyQueue.xml"));

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/related");

            content.Headers.ContentType.Parameters.Add(
                new System.Net.Http.Headers.NameValueHeaderValue("boundary", "f438a15e-9b5b-491f-9b47-aba4d00b8837"));

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse
                .ReadSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>()
                .ConfigureAwait(false);

            Assert.IsNull(response.ResponseMessage.Response);
        }
    }
}
