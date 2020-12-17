using System;

using Microsoft.Win32.SafeHandles;

namespace CAPILite
{
    public class HashSafeHandle: SafeHandleZeroOrMinusOneIsInvalid
    {
        protected HashSafeHandle()
        : base(true)
        {
        }        

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            var result = CApiLiteNative.CryptDestroyHash(handle);

            handle = IntPtr.Zero;

            return result;
        }
    }
}
