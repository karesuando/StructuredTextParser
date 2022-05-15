using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ErrorManager
{
    public class STLangCompilerError : Exception
    {
        public STLangCompilerError(string msg)
            : base(MakeMessage(msg))
        {
        }

        public STLangCompilerError(string msg, object parameter)
            : base(MakeMessage(msg, parameter))
        {
        }

        public STLangCompilerError(string msg, object param1, object param2)
            : base(MakeMessage(msg, param1, param2))
        {
        }

        private static string MakeMessage(string msg)
        {
            return "Compiler error: " + msg;
        }

        private static string MakeMessage(string msg, object param)
        {
            return string.Format("Compiler error: " + msg, param);
        }

        private static string MakeMessage(string msg, object param1, object param2)
        {
            return string.Format("Compiler error: " + msg, param1, param2);
        }
    }
}
