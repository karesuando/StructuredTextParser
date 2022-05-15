using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class IntExtension
    {
        public static byte[] GetBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }
    }

    public static class UIntExtension
    {
        public static byte[] GetBytes(this uint value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
