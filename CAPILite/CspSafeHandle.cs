using System;

using Microsoft.Win32.SafeHandles;

namespace CAPILite
{
    public class CspSafeHandle: SafeHandleZeroOrMinusOneIsInvalid
    {
        protected CspSafeHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return true;
            }

            var result = CApiLiteNative.CryptReleaseContext(handle, 0);
            if (result)
            {
                SetHandleAsInvalid();
            }

            return result;
        }
    }
}
