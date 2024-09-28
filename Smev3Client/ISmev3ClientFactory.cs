namespace Smev3Client
{
    public interface ISmev3ClientFactory
    {
        /// <summary>
        /// Создание клиента по мнемонике сервиса
        /// </summary>
        /// <param name="mnemonic">Мнемоника сервиса в СМЭВ</param>
        ISmev3Client Get(string mnemonic);
    }
}
