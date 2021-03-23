using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Smev3Client.Tests
{
    [TestClass]
    public class Smev3ClientResponseTests
    {
        [TestMethod]
        public void IsErrorResponce()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var smevResponse = new Smev3ClientResponse(httpResponse);

            Assert.IsTrue(smevResponse.IsErrorResponse);
        }

        [TestMethod]
        public async Task ReadShortErrorInfo()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            { 
                Content = new StringContent(File.ReadAllText("TestData/SoapFaultResponse.xml")) 
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var errorInfo = await smevResponse.ReadAsSmev3ErrorInfoAsync();

            Assert.IsNotNull(errorInfo?.Message);
        }

        [TestMethod]
        public async Task ReadDetailErrorInfo()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(File.ReadAllText("TestData/SignatureVerificationFaultResponse.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var errorInfo = await smevResponse.ReadAsSmev3ErrorInfoAsync();

            Assert.IsNotNull(errorInfo?.SmevCode);
        }

        [TestMethod]
        public async Task ReadSendRequestResponse()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(File.ReadAllText("TestData/SendRequestResponce.xml"))
            };

            var smevResponse = new Smev3ClientResponse(httpResponse);

            var errorInfo = await smevResponse.ReadAsSmev3ErrorInfoAsync();

            Assert.IsNotNull(errorInfo?.SmevCode);
        }
    }
}
