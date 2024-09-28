using System;

namespace Smev3Client
{
    public class SmevServiceConfig
    {
        public SmevServiceConfig()
        {
        }

        public SmevServiceConfig(SmevServiceConfig src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            Container = src.Container;
            Password = src.Password;
            Thumbprint = src.Thumbprint;
            Mnemonic = src.Mnemonic;
        }

        /// <summary>
        /// Мнемоника сервиса
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// Путь к pfx файлу
        /// </summary>
        public string Container { get; set; }

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
