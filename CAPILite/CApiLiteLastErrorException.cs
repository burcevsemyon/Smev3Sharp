using System;
using System.Collections.Generic;
using System.Text;

namespace CAPILite
{
    public class CApiLiteLastErrorException: Exception
    {
        public CApiLiteLastErrorException():
            base(GetMessage(CApiLiteNative.GetLastError()))
        {
        }

        static string GetMessage(int errorCode)
        {
            var buffer = new StringBuilder(512);
            var result = CApiLiteNative.FormatMessage(
                CApiLiteConsts.FORMAT_MESSAGE_IGNORE_INSERTS 
                | CApiLiteConsts.FORMAT_MESSAGE_FROM_SYSTEM 
                | CApiLiteConsts.FORMAT_MESSAGE_FROM_HMODULE,
                IntPtr.Zero, errorCode, 0, buffer, buffer.Capacity, IntPtr.Zero);
            if (result != 0)
            {
                return buffer.ToString();
            }

            return "Unknown error code: 0x" + errorCode.ToString("X8");
        }
    }
}
