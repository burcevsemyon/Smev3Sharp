using System;
using System.Runtime.Serialization;

namespace Smev3Client
{
    public class Smev3ClientException : Exception
    {        
        public Smev3ClientException()
        {
        }

        public Smev3ClientException(string message) : base(message)
        {
        }

        public Smev3ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Smev3ClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
