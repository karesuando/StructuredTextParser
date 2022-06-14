using System;
using System.Runtime.InteropServices;
using STLang.WrapperClass;

namespace StructuredTextTester
{
    class Program
    {
        [DllImport(@"C:\Users\cyber\Source\Repos\StructuredTextRuntime\Debug\StructuredTextRuntime.dll")]
        public static extern IntPtr CreatePOUObject(string str);

        [DllImport(@"C:\Users\cyber\Source\Repos\StructuredTextRuntime\Debug\StructuredTextRuntime.dll")]
        public static extern double ExecutePOU(double[] argV, int argC, IntPtr pouObj);
        static void Main(string[] args)
        {
            IntPtr f2;
            StructuredText stlang = new StructuredText();
            string[] v =
            {
               "Let's presume the plane did go down in the Indian Ocean",
               "The U.S. Navy has sent a towed pinger locator, or TPL",
               "In 2009, the Phoenix TPL-25, in conjunction with technology",
               "Woods Hole Oceanographic Institute, searched for a ping",
               "That search didn't find the plane, but two years later",
               "searchers found the flight data recorder and the bulk",
               "AUVs are normally used in the oil and gas industry to",
               "The smaller ones are only going to go down to about",
               "technology has greatly improved the search for answers",
               "One of the most sophisticated AUVs owned by Phoenix"
            };
            int[] d = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Array.Sort(v, StringComparer.Ordinal);
            bool success = stlang.TryParse(".", "ST.st");
            if (success)
            {
                f2 = CreatePOUObject("ST.xstl");
                double[] argV = { 1.0 };
                double result = ExecutePOU(argV, 1, f2);
            }
        }
    }
}
