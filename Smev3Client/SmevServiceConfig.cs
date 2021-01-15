namespace Smev3Client
{
    public class SmevServiceConfig 
    {
        public SmevServiceConfig()
        {            
        }

        public SmevServiceConfig(SmevServiceConfig src)
        {
            Container = src?.Container;
            Password = src?.Password;
            Thumbprint = src?.Thumbprint;
        }

        /// <summary>
        /// Путь к pfx файлу
        /// </summary>
        public string  Container { get; set; }
        
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Отпечаток сертификата
        /// </summary>
        public string Thumbprint { get; set; }
    }
}
