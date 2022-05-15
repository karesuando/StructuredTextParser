using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Extensions
{
    public static class TimeSpanExtension
    {
        public static byte[] GetBytes(this TimeSpan timeSpan)
        {
            return timeSpan.Ticks.GetBytes();
        }
    }
}
