using System.Net.Http;

using Smev3Client.Soap;

namespace Smev3Client
{
    public class Smev3ClientResponse<T> : Smev3ClientResponse
        where T : ISoapEnvelopeBody, new()
    {
        /// <summary>
        /// Десериализованное тело ответа
        /// </summary>
        public T Data { get; private set; }

        public Smev3ClientResponse(HttpResponseMessage response, T data)
            : base(response)
        {
            Data = data;
        }
    }
}
