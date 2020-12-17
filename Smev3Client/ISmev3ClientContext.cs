using System;

namespace Smev3Client
{
    public interface ISmev3ClientContext
    {
        /// <summary>
        /// Адрес стенда СМЭВ3
        /// </summary>
        Uri ServiceUri { get; }
    }
}
