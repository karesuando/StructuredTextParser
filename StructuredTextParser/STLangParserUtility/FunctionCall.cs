using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.ParserUtility
{
    public class STLangFunctionCall : ProgramOrganizationUnitCall
    {
        public STLangFunctionCall(STLangSymbol symbol, LexLocation location)
            : base(symbol, location)
        {
        }

        public STLangFunctionCall(STLangSymbol symbol, POUParameter argument, LexLocation location)
            : base(symbol, argument, location)
        {
            this.position = 0;
        }

        public override void Add(POUParameter argument)
        {
            if (argument != null)
            {
                this.arguments.Add(argument);
                if (!argument.IsInputArgAssignment)
                    argument.Position = this.position++;
            }
        }

        public override bool IsFunction
        {
            get { return true; }
        }

        private int position;
    }
}
