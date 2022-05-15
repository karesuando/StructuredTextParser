using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class ShortExtension
    {
        public static byte[] GetBytes(this short value)
        {
            return BitConverter.GetBytes(value);
        }
    }

    public static class UShortExtension
    {
        public static byte[] GetBytes(this ushort value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}