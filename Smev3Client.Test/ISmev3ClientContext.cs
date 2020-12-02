using System;

namespace Smev3Client.Test
{
    public interface ISmev3ClientContext
    {
        /// <summary>
        /// Адрес стенда СМЭВ3
        /// </summary>
        Uri ServiceUri { get; }
    }
}
