using System;

namespace Smev3Client.Test
{

    public class Smev3ClientContext: ISmev3ClientContext
    {
        public Smev3ClientContext(Uri serviceUri)
        {
            ServiceUri = serviceUri ?? throw new ArgumentNullException(nameof(serviceUri));
        }

        public Uri ServiceUri { get; }
    }
}
