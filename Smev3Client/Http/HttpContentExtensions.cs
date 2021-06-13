using System.Xml;
using System.Xml.Serialization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Smev3Client.Soap;

namespace Smev3Client.Http
{
    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadContentSoapBodyAsAsync<T>(
            this HttpContent httpContent, CancellationToken cancellationToken)
            where T : ISoapEnvelopeBody, new()
        {
            HttpContent localHttpContent;
            if (httpContent.IsMimeMultipartContent())
            {
                var multipartStreamProvider = await httpContent.ReadAsMultipartAsync(cancellationToken)
                                                                        .ConfigureAwait(false);

                localHttpContent = multipartStreamProvider.Contents[0];
            }
            else
            {
                localHttpContent = httpContent;
            }

            var stream = await localHttpContent.ReadAsStreamAsync()
                                            .ConfigureAwait(false);

            if (stream.CanSeek)
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
            }

            var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true
            });

            var serializer = new XmlSerializer(typeof(SoapEnvelope<T>));

            var envelope = (SoapEnvelope<T>)serializer.Deserialize(reader);

            return envelope.Body;
        }
    }
}
