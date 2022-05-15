using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class StringExtension
    {
        public static string RemoveChar(this string str, char ch)
        {
            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] == ch)
                    str = str.Remove(i, 1);
            }
            return str;
        }

        public static byte[] GetAscIIBytes(this string stringValue)
        {
            ASCIIEncoding ascII = new ASCIIEncoding();
            List<byte> ascIIString = new List<byte>();
            ascIIString.AddRange(ascII.GetBytes(stringValue));
            ascIIString.Add(0x00);
            return ascIIString.ToArray();
        }

        public static byte[] GetUnicodeChars(this string stringValue)
        {
            List<byte> bytes = new List<byte>();

            foreach (char ch in stringValue)
                bytes.AddRange(BitConverter.GetBytes(ch));
            bytes.AddRange(new byte[2] { 0x00, 0x00 });
            return bytes.ToArray();
        }

        public static byte[] GetAscIIBytes(this string stringValue, int size)
        {
            ASCIIEncoding ascII = new ASCIIEncoding();
            List<byte> ascIIString = new List<byte>();
            ascIIString.AddRange(ascII.GetBytes(stringValue));
            for (int i = stringValue.Length; i < size; i++)
                ascIIString.Add(0x00); // Pad with ascii null bytes
            return ascIIString.ToArray();
        }

        public static byte[] GetUnicodeChars(this string stringValue, int size)
        {
            List<byte> bytes = new List<byte>();

            foreach (char ch in stringValue)
                bytes.AddRange(BitConverter.GetBytes(ch));
            for (int i = stringValue.Length; i < size; i++)
                bytes.AddRange(new byte[2] { 0x00, 0x00 }); // Pad with unicode null bytes
            return bytes.ToArray();
        }
    }
}
