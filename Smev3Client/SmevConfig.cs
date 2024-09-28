using System;
using System.Collections.Generic;

namespace Smev3Client
{
    public class SmevConfig
    {
        /// <summary>
        /// Url стенда
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Дескрипторы сервисов
        /// </summary>
        public List<SmevServiceConfig> ServiceConfigs { get; set; }
    }
}
