using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smev3Client
{

    public class Smev3Client :
        IDisposable, ISmev3Client
    {
        #region members

        /// <summary>
        /// Параметры клиента
        /// </summary>
        private readonly ISmev3ClientContext _context;

        private readonly HttpClient _httpClient = new HttpClient();

        #endregion        

        public Smev3Client(ISmev3ClientContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _httpClient.BaseAddress = _context.ServiceUri;
        }

        /// <summary>
        /// Отправка конверта
        /// </summary>
        /// <param name="requestData"></param>
        public async Task<Smev3ClientResponse> SendAsync(
            ISmev3Envelope envelope,
            CancellationToken cancellationToken)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            var envelopeBytes = envelope.Get();

            var encoding = new UTF8Encoding(true);

            var str = encoding.GetString(envelopeBytes);

            var content = new ByteArrayContent(
                envelopeBytes, 0, envelopeBytes.Length);

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Xml)
            {
                CharSet = "utf-8"
            };

            cancellationToken.ThrowIfCancellationRequested();

            var response = await _httpClient.PostAsync(
                string.Empty,
                content,
                cancellationToken);

            return new Smev3ClientResponse
            {
                HttpResponse = response
            };
        }

        #region IDisposable

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion        
    }
}
