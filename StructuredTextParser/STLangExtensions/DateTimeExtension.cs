using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class DateTimeExtension
    {
        public static byte[] GetBytes(this DateTime dateTime)
        {
            return dateTime.Ticks.GetBytes();
        }
    }
}
