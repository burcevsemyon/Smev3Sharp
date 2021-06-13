using System;
using System.Runtime.Serialization;

using Smev3Client.Soap;

namespace Smev3Client
{
    public class Smev3Exception : Exception
    {
        public SoapFault FaultInfo { get; set; }

        public Smev3Exception()
        {
        }

        public Smev3Exception(string message) : base(message)
        {
        }

        public Smev3Exception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Smev3Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
