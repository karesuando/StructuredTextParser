using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;
using STLang.Scanner;
using STLang.ErrorManager;
using STLang.RuntimeWrapper;
using System.Runtime.InteropServices;
using System.IO;

namespace STLang.WrapperClass
{
    public class StructuredText
    {
        public StructuredText()
        {
            this.errorHandler = new ErrorHandler();
        }

        [DllImport("STLangRuntime", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[DllImport("libSTLangRuntime.so", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern bool Load();

        public bool LoadDll(out String ErrMsg)
        {
            ErrMsg = "";
            try
            {
                return (Load());
            }
            catch(Exception e)
            {
                ErrMsg = e.Message;
                return (false);
            }
        }


        public bool TryParse(string pouString, out STLangPOUObject pouObjectHandle, bool overWrite = false)
        {
            this.errorHandler = new ErrorHandler();
            STLangScanner scanner = new STLangScanner(pouString, errorHandler);
            STLangParser parser = new STLangParser(scanner, errorHandler);
            bool result = parser.Parse();
            if (!result || parser.Errors > 0)
            {
                pouObjectHandle = null;
                return false;
            }
            else
            {
                pouObjectHandle = parser.GenerateObjectCode(overWrite);
                return true;
            }
        }

        public bool Parse(string pouString)
        {
            this.errorHandler = new ErrorHandler();
            STLangScanner scanner = new STLangScanner(pouString, errorHandler);
            STLangParser parser = new STLangParser(scanner, errorHandler);
            return parser.Parse();
        }

        public bool TryParse(string path, string sourceFile)
        {
            try
            {
                using (Stream stream = File.Open(sourceFile, FileMode.Open))
                {
                    this.errorHandler = new ErrorHandler();
                    STLangScanner scanner = new STLangScanner(stream, errorHandler);
                    STLangParser parser = new STLangParser(scanner, errorHandler);
                    return parser.Parse(path, sourceFile);
                }
            }
            catch (DirectoryNotFoundException e)
            {
            }
            catch (FileNotFoundException e)
            {
            }
            catch (ArgumentException e)
            {
            }
            catch (IOException e)
            {
            }
            return false;
        }

        public IEnumerable<string> ErrorMessages
        {
            get { return this.errorHandler.ErrorMessages; }
        }

        public IEnumerable<string> WarningMessages
        {
            get { return this.errorHandler.WarningMessages; }
        }

        public int Errors
        {
            get { return this.errorHandler.Errors; }
        }

        public int Warnings
        {
            get { return this.errorHandler.Warnings; }
        }

        private ErrorHandler errorHandler;
    }
}
