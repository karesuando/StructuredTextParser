using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ParserUtility;
using STLang.ConstantTokens;

namespace STLang.Extensions
{
    public static class ListExtension
    {
        public delegate T Converter<T>(object value);

        public static List<T> ConvertTo<T>(this List<Expression> expressionList, Converter<T> converter)
        {
           return expressionList.ConvertAll(new Converter<Expression, T>(e => converter(e.Evaluate())));
        }

        public static List<T> Succ<T>(this List<T> list)
        {
            return list.Skip(1).ToList();
        }

        public static int MaxIndex<T>(this List<T> comparableValue) where T: IComparable<T>
        {
            int index = 0;
            int maxIndex = 0;
            T maxValue = comparableValue[0];
            foreach (T value in comparableValue.Skip(1))
            {
                index++;
                if (value.CompareTo(maxValue) > 0)
                {
                    maxValue = value;
                    maxIndex = index;
                }
            }
            return maxIndex;
        }

        public static int MinIndex<T>(this List<T> comparableValue) where T : IComparable<T>
        {
            int index = 0;
            int minIndex = 0;
            T minValue = comparableValue[0];
            foreach (T value in comparableValue.Skip(1))
            {
                index++;
                if (value.CompareTo(minValue) < 0)
                {
                    minValue = value;
                    minIndex = index;
                }
            }
            return minIndex;
        }

        public static bool IsUnique(this List<string> identList, string ident)
        {
            StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;
            return identList.Find(id => id.Equals(ident, ignoreCase)) == null;
        }

        public static Expression[] ToArray(this List<InputParameter> inputs)
        {
            return (from input in inputs
                    select input.RValue).ToArray();
        }

        private static byte[] Flatten(this IEnumerable<byte[]> byteArrayList)
        {
            return (from byteArray in byteArrayList 
                    select byteArray).SelectMany(b => b).ToArray();
        }

        public static byte[] GetBytes(this List<byte[]> byteArrayList)
        {
            return byteArrayList.Flatten();
        }

        public static int[] ToIntArray(this List<uint> uintList)
        {
            return uintList.ConvertAll(new Converter<uint,int>(e => (int)e)).ToArray();
        }

        public static byte[] GetBytes(this List<double> doubleList)
        {
            return (from doubleValue in doubleList
                    select doubleValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<float> floatList)
        {
            return (from floatValue in floatList
                    select floatValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<int> intList)
        {
            return (from intValue in intList
                    select intValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<uint> uintList)
        {
            return (from uintValue in uintList
                    select uintValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<long> longList)
        {
            return (from longValue in longList
                    select longValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<ulong> ulongList)
        {
            return (from ulongValue in ulongList
                    select ulongValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<short> shortList)
        {
            return (from shortValue in shortList
                    select shortValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<sbyte> sbyteList)
        {
            return (from sbyteValue in sbyteList
                    select (byte)sbyteValue).ToArray();
        }

        public static byte[] GetBytes(this List<bool> boolList)
        {
            return (from boolValue in boolList
                    select boolValue ? (byte)1 : (byte)0).ToArray();
        }

        public static byte[] GetBytes(this List<byte> byteList)
        {
            return byteList.ToArray();
        }

        public static byte[] GetBytes(this List<ushort> ushortList)
        {
            return (from ushortValue in ushortList
                    select ushortValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<DateTime> dateTimeList)
        {
            return (from dateTimeValue in dateTimeList
                    select dateTimeValue.GetBytes()).Flatten();
        }

        public static byte[] GetBytes(this List<TimeSpan> timeSpanList)
        {
            return (from timeSpanValue in timeSpanList
                    select timeSpanValue.GetBytes()).Flatten();
        }

        public static byte[] GetAscIIBytes(this List<string> stringList, int size)
        {
            return (from stringValue in stringList
                    select stringValue.GetAscIIBytes(size)).Flatten();
        }

        public static byte[] GetUnicodeBytes(this List<string> stringList, int size)
        {
            return (from stringValue in stringList
                    select stringValue.GetUnicodeChars(size)).Flatten();
        }
    }
}
