using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smev3Client
{
    public interface ISmev3Client
    {
        /// <summary>
        /// Отправка запроса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Smev3ClientResponse> SendRequestAsync<T>(SendRequestExecutionContext<T> context, 
                                                            CancellationToken cancellationToken) where T: new();

        /// <summary>
        /// Получение ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Smev3ClientResponse> GetResponseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Подтверждение получения ответа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Smev3ClientResponse> AckAsync(Guid messageId, CancellationToken cancellationToken);
    }
}