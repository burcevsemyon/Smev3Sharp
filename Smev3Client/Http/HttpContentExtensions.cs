using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.WebUtilities;

using Smev3Client.Soap;

namespace Smev3Client.Http
{
    internal static class HttpContentExtensions
    {
        internal static async Task<T> ReadSoapBodyAsAsync<T>(
            this HttpContent httpContent, CancellationToken cancellationToken)
            where T : ISoapEnvelopeBody, new()
        {
            var stream = await httpContent.ReadAsStreamAsync()
                                                .ConfigureAwait(false);

            if (httpContent.IsMimeMultipartContent(out string boundary))
            {
                var multipartReader = new MultipartReader(boundary, stream);
             
                var section = await multipartReader.ReadNextSectionAsync(cancellationToken)
                                                .ConfigureAwait(false);

                await section.Body.DrainAsync(cancellationToken)
                    .ConfigureAwait(false);

                stream = section.Body;
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
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

        internal static bool IsMimeMultipartContent(this HttpContent httpContent, out string boundary)
        {
            boundary = null;

            if (!httpContent.Headers.ContentType.MediaType.StartsWith("multipart"))
            {
                return false;
            }

            foreach (var parameter in httpContent.Headers.ContentType.Parameters)
            {
                if (parameter.Name.Equals("boundary"))
                {
                    boundary = parameter.Value;

                    return true;
                }
            }

            return false;
        }
    }
}
