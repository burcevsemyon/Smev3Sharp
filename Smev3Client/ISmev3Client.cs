using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smev3Client
{
    public interface ISmev3Client: IDisposable
    {
        /// <summary>
        /// Отправка пакета СМЭВ3
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Smev3ClientResponse> SendAsync(ISmev3Envelope envelope, CancellationToken cancellationToken);        
    }
}