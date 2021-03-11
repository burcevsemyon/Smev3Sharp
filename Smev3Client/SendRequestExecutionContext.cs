namespace Smev3Client
{
    public class SendRequestExecutionContext<T> where T: new()
    {
        /// <summary>
        /// Данные запроса
        /// </summary>
        public T RequestData { get; set; }

        /// <summary>
        /// Флаг тестового запроса
        /// </summary>
        public bool IsTest { get; set; }
    }
}
