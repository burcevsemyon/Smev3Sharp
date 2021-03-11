using Smev3Client.Soap;

namespace Smev3Client
{
    public class Smev3ErrorInfo
    {
        public Smev3ErrorInfo(SoapFault soapFault = null)
        {
            Code = soapFault?.FaultCode;
            Message = soapFault?.FaultString;
            SmevCode = soapFault?.DetailCode;
            SmevMessage = soapFault?.DetailDescription;
        }

        public string Code { get; set; }

        public string Message { get; set; }

        public string SmevCode { get; set; }

        public string SmevMessage { get; set; }
    }
}
