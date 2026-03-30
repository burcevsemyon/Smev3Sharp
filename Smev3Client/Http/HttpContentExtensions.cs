using System;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.WebUtilities;

using Smev3Client.Soap;

namespace Smev3Client.Http
{
    internal static class HttpContentExtensions
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializersCache = new ConcurrentDictionary<Type, XmlSerializer>();
        
        internal static async Task<T> ReadSoapBodyAsAsync<T>(
            this HttpContent httpContent, CancellationToken cancellationToken)
            where T : ISoapEnvelopeBody, new()
        {
            await using var stream = await httpContent.ReadSoapBodyAsStreamAsync(cancellationToken)
                                                  .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };
            
            var serializer = SerializersCache.GetOrAdd(typeof(SoapEnvelope<T>), type => new XmlSerializer(type));

            using var reader = XmlReader.Create(stream, readerSettings);

            var envelope = (SoapEnvelope<T>)serializer.Deserialize(reader);

            return envelope.Body;
        }

        internal static async Task<string> ReadSoapBodyAsStringAsync(
            this HttpContent httpContent, CancellationToken cancellationToken)
        {
            await using var stream = await httpContent
                                            .ReadSoapBodyAsStreamAsync(cancellationToken)
                                            .ConfigureAwait(false);
            
            using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            return await streamReader.ReadToEndAsync();
        }

        private static async Task<Stream> ReadSoapBodyAsStreamAsync(
                    this HttpContent httpContent, CancellationToken cancellationToken)
        {
            Stream contentStream = null;
            try
            {
                contentStream = await httpContent
                                    .ReadAsStreamAsync()
                                    .ConfigureAwait(false);
                
                contentStream.SeekToBeginIfPossible();

                if (!httpContent.IsMimeMultipartContent(out var boundary))
                {
                    return contentStream;
                }
                
                var multipartReader = new MultipartReader(boundary, contentStream);

                var section = await multipartReader
                                                .ReadNextSectionAsync(cancellationToken)
                                                .ConfigureAwait(false);
                if (section != null)
                {
                    return section.Body.SeekToBeginIfPossible();
                }
                
                await contentStream.DisposeAsync();
                
                return new MemoryStream(Array.Empty<byte>(), false);
            }
            catch
            {
                if (contentStream != null)
                {
                    await contentStream.DisposeAsync();
                }
                throw;
            }
        }

        private static bool IsMimeMultipartContent(this HttpContent httpContent, out string boundary)
        {
            boundary = null;

            var contentType = httpContent.Headers.ContentType;
            if (contentType?.MediaType?.StartsWith("multipart", StringComparison.OrdinalIgnoreCase) != true)
            {
                return false;
            }

            var param = contentType.Parameters.FirstOrDefault(i =>
                i.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase));

            boundary = param?.Value?.Trim(' ').Trim('"');
            return string.IsNullOrWhiteSpace(boundary) ? throw
                // RFC: multipart/* requires a boundary parameter, otherwise the payload cannot be reliably parsed.
                new InvalidOperationException("Invalid multipart content: missing required 'boundary' parameter in Content-Type.") : true;
        }

        private static Stream SeekToBeginIfPossible(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.CanSeek && stream.Position != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }
    }
}
