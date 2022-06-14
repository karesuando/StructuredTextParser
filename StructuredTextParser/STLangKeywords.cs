using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;


namespace STLang.KeyWords
{
    public class STLangKeywordDictionary : Dictionary<string, Tokens>
    {
        public STLangKeywordDictionary()
        {
            this.Add("REPEAT", Tokens.REPEAT);
            this.Add("UNTIL", Tokens.UNTIL);
            this.Add("FOR", Tokens.FOR);
            this.Add("IF", Tokens.IF);
            this.Add("TO", Tokens.TO);
            this.Add("DO", Tokens.DO);
            this.Add("THEN", Tokens.THEN);
            this.Add("ELSE", Tokens.ELSE);
            this.Add("CASE", Tokens.CASE);
            this.Add("END_CASE", Tokens.END_CASE);
            this.Add("ELSIF", Tokens.ELSIF);
            this.Add("END_FOR", Tokens.END_FOR);
            this.Add("WHILE", Tokens.WHILE);
            this.Add("END_WHILE", Tokens.END_WHILE);
            this.Add("END_IF", Tokens.END_IF);
            this.Add("END_REPEAT", Tokens.END_REPEAT);
            this.Add("EXIT", Tokens.EXIT);
            this.Add("BY", Tokens.BY);
            this.Add("PROGRAM", Tokens.PROGRAM);
            this.Add("END_PROGRAM", Tokens.END_PROGRAM);
            this.Add("ARRAY", Tokens.ARRAY);
            this.Add("OF", Tokens.OF);
            this.Add("STRUCT", Tokens.STRUCT);
            this.Add("END_STRUCT", Tokens.END_STRUCT);
            this.Add("FUNCTION", Tokens.FUNCTION);
            this.Add("FUNCTION_BLOCK", Tokens.FUNCTION_BLOCK);
            this.Add("END_FUNCTION", Tokens.END_FUNCTION);
            this.Add("END_FUNCTION_BLOCK", Tokens.END_FUNCTION_BLOCK);
            this.Add("TYPE", Tokens.TYPE);
            this.Add("END_TYPE", Tokens.END_TYPE);
            this.Add("INT", Tokens.INT);
            this.Add("SINT", Tokens.SINT);
            this.Add("DINT", Tokens.DINT);
            this.Add("LINT", Tokens.LINT);
            this.Add("USINT", Tokens.USINT);
            this.Add("UINT", Tokens.UINT);
            this.Add("UDINT", Tokens.UDINT);
            this.Add("ULINT", Tokens.ULINT);
            this.Add("REAL", Tokens.REAL);
            this.Add("LREAL", Tokens.LREAL);
            this.Add("DATE", Tokens.DATE);
            this.Add("TIME", Tokens.TIME);
            this.Add("TIME_OF_DAY", Tokens.TIME_OF_DAY);
            this.Add("TOD", Tokens.TOD);
            this.Add("DATE_AND_TIME", Tokens.DATE_AND_TIME);
            this.Add("DT", Tokens.DT);
            this.Add("STRING", Tokens.STRING);
            this.Add("WSTRING", Tokens.WSTRING);
            this.Add("BOOL", Tokens.BOOL);
            this.Add("BYTE", Tokens.BYTE);
            this.Add("CHAR", Tokens.CHAR);
            this.Add("WCHAR", Tokens.WCHAR);
            this.Add("WORD", Tokens.WORD);
            this.Add("DWORD", Tokens.DWORD);
            this.Add("LWORD", Tokens.LWORD);
            this.Add("ANY", Tokens.ANY);
            this.Add("ANY_INT", Tokens.ANY_INT);
            this.Add("ANY_REAL", Tokens.ANY_REAL);
            this.Add("ANY_BIT", Tokens.ANY_BIT);
            this.Add("ANY_NUM", Tokens.ANY_NUM);
            this.Add("ANY_DATE", Tokens.ANY_DATE);
            this.Add("ANY_DERIVED", Tokens.ANY_DERIVED);
            this.Add("ANY_ELEMENTARY", Tokens.ANY_ELEMENTARY);
            this.Add("ANY_MAGNITUDE", Tokens.ANY_MAGNITUDE);
            this.Add("ANY_STRING", Tokens.ANY_STRING);
            this.Add("R_EDGE", Tokens.R_EDGE);
            this.Add("F_EDGE", Tokens.F_EDGE);
            this.Add("VAR", Tokens.VAR);
            this.Add("VAR_INPUT", Tokens.VAR_INPUT);
            this.Add("VAR_OUTPUT", Tokens.VAR_OUTPUT);
            this.Add("VAR_IN_OUT", Tokens.VAR_IN_OUT);
            this.Add("VAR_GLOBAL", Tokens.VAR_GLOBAL);
            this.Add("VAR_ACCESS", Tokens.VAR_ACCESS);
            this.Add("VAR_CONFIG", Tokens.VAR_CONFIG);
            this.Add("VAR_EXTERNAL", Tokens.VAR_EXTERNAL);
            this.Add("VAR_TEMP", Tokens.VAR_TEMP);
            this.Add("END_VAR", Tokens.END_VAR);
            this.Add("AT", Tokens.AT);
            this.Add("CONSTANT", Tokens.CONSTANT);
            this.Add("OVERLAP", Tokens.OVERLAP);
            this.Add("RETAIN", Tokens.RETAIN);
            this.Add("TRUE", Tokens.TRUE);
            this.Add("FALSE", Tokens.FALSE);
            this.Add("RETURN", Tokens.RETURN);
            this.Add("READ_ONLY", Tokens.READ_ONLY);
            this.Add("WRITE_ONLY", Tokens.WRITE_ONLY);
            this.Add("READ_WRITE", Tokens.READ_WRITE);
            this.Add("OR", Tokens.IOR);
            this.Add("XOR", Tokens.XOR);
            this.Add("AND", Tokens.AND);
            this.Add("NOT", Tokens.NOT);
            this.Add("MOD", Tokens.MOD);
            this.Add("CONFIGURATION", Tokens.CONFIGURATION);
            this.Add("END_CONFIGURATION", Tokens.END_CONFIGURATION);
            this.Add("INITIAL_STEP", Tokens.INITIAL_STEP);
            this.Add("END_STEP", Tokens.END_STEP);
            this.Add("RESOURCE", Tokens.RESOURCE);
            this.Add("END_RESOURCE", Tokens.END_RESOURCE);
            this.Add("TRANSITION", Tokens.TRANSITION);
            this.Add("END_TRANSITION", Tokens.END_TRANSITION);
            this.Add("WITH", Tokens.WITH);
            this.Add("TASK", Tokens.TASK);
            this.Add("(*WHEN*)", Tokens.WHEN); // Dummy token used to mark beginning of a case_element
        }

        public bool IsKeyword(string text, out Tokens token)
        {
            return this.TryGetValue(text.ToUpper(), out token);
        }
    }
}
