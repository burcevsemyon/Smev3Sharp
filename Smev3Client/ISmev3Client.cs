using System;
using System.Threading;
using System.Threading.Tasks;

using Smev3Client.Smev;
using Smev3Client.Soap;

namespace Smev3Client
{
    public interface ISmev3Client
    {
        /// <summary>
        /// Отправка запроса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">Параметры методы</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        Task<Smev3ClientResponse<SendRequestResponse>> SendRequestAsync<T>(SendRequestExecutionContext<T> context,
                                                            CancellationToken cancellationToken)
            where T : new();

        /// <summary>
        /// Получение ответа
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        Task<Smev3ClientResponse> GetResponseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Подтверждение получения ответа
        /// </summary>
        /// <param name="messageId">Ид. подтверждаемого сообщения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        Task<Smev3ClientResponse> AckAsync(Guid messageId, CancellationToken cancellationToken);
    }
}