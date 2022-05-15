using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class LongExtension
    {
        public static byte[] GetBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }
    }

    public static class ULongExtension
    {
        public static byte[] GetBytes(this ulong value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
