using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class FloatExtension
    {
        public static byte[] GetBytes(this float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static string GetBinaryString(this float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt32(bytes, 0).ToString();
        }
    }
}
