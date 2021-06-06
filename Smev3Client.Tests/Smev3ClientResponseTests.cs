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

            var soapFault = await smevResponse.ReadContentSoapBodyAsAsync<SoapFault>();

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

            var soapFault = await smevResponse.ReadContentSoapBodyAsAsync<SoapFault>();

            Assert.IsNotNull(soapFault.DetailCode);
        }

        [TestMethod]
        public async Task ReadSendRequestResponseMessageId_Exists()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponce.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var response = await smevResponse.ReadContentSoapBodyAsAsync<SendRequestResponse>();

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

            var response = await smevResponse.ReadContentSoapBodyAsAsync<SendRequestResponse>();

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

            var response = await smevResponse.ReadContentSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>();

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

            var response = await smevResponse.ReadContentSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>();

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

            var response = await smevResponse.ReadContentSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>();

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

            var response = await smevResponse.ReadContentSoapBodyAsAsync<GetResponseResponse<FakeSmevServiceResponse>>();

            Assert.IsNull(response.ResponseMessage.Response);
        }
    }
}
