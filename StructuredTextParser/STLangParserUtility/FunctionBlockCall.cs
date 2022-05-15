using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.Symbols;
using STLang.Expressions;
using QUT.Gppg;
using STLang.ParserUtility;
using STLang.Statements;

namespace STLang.ParserUtility
{
    public class STLangFunctionBlockCall : ProgramOrganizationUnitCall
    {
        public STLangFunctionBlockCall(STLangSymbol symbol, LexLocation location)
            : base(symbol, location)
        {
          
        }

        public STLangFunctionBlockCall(STLangSymbol symbol, POUParameter argument, LexLocation location)
            : base(symbol, argument, location)
        {
          
        }

        public override bool IsFunctionBlock
        {
            get { return true; }
        }
    }
}
