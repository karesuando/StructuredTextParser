using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QUT.Gppg;
using STLang.Expressions;

namespace STLang.Symbols
{
    public class BCDConverterSymbol : STLangSymbol
    {
        public BCDConverterSymbol(string name)
            : base(name)
        {
        }

        public override string TypeName
        {
            get { return "typkonverteringsfunktion"; }
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc = null)
        {
            throw new NotImplementedException();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> argList, LexLocation loc = null)
        {
            throw new NotImplementedException();
        }
    }
}
