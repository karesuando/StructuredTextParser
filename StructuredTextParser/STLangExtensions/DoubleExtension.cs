using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class DoubleExtension
    {
        public static byte[] GetBytes(this double value)
        {
            return BitConverter.GetBytes(value);
        }

        public static string GetBinaryString(this double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0).ToString();
        }
    }
}
