using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
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
        private static readonly XmlReaderSettings XmlReaderSettings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        
        private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializersCache = new ConcurrentDictionary<Type, XmlSerializer>();
        
        internal static async Task<T> ReadSoapBodyAsAsync<T>(
            this HttpContent httpContent, CancellationToken cancellationToken)
            where T : ISoapEnvelopeBody, new()
        {
            using var stream = await httpContent.ReadSoapBodyAsStreamAsync(cancellationToken)
                                                  .ConfigureAwait(false);
            
            var serializer = SerializersCache.GetOrAdd(typeof(SoapEnvelope<T>), type => new XmlSerializer(type));

            using var reader = XmlReader.Create(stream, XmlReaderSettings);

            var envelope = (SoapEnvelope<T>)serializer.Deserialize(reader);

            return envelope.Body;
        }

        internal static async Task<string> ReadSoapBodyAsStringAsync(
            this HttpContent httpContent, CancellationToken cancellationToken)
        {
            using var stream = await httpContent
                                            .ReadSoapBodyAsStreamAsync(cancellationToken)
                                                .ConfigureAwait(false);
            
            using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            return await streamReader.ReadToEndAsync()
                                        .ConfigureAwait(false);
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

                if (!httpContent.TryGetMultipartContentBoundary(out var boundary))
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
                
                await contentStream.DisposeAsync()
                                    .ConfigureAwait(false);
                
                return new MemoryStream(Array.Empty<byte>(), false);
            }
            catch
            {
                if (contentStream != null)
                {
                    await contentStream.DisposeAsync()
                                        .ConfigureAwait(false);
                }
                throw;
            }
        }

        private static bool TryGetMultipartContentBoundary(this HttpContent httpContent, [NotNullWhen(true)] out string? boundary)
        {
            boundary = null;

            var contentType = httpContent.Headers.ContentType;
            if (contentType?.MediaType?.StartsWith("multipart", StringComparison.OrdinalIgnoreCase) != true)
            {
                return false;
            }

            foreach (var parameter in contentType.Parameters)
            {
                if (!parameter.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                boundary = parameter.Value?.Trim(' ').Trim('"');
                break;
            }
            
            return string.IsNullOrWhiteSpace(boundary) ? throw
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
