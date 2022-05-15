using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.ParserUtility
{
    public class OutputParameter : POUParameter
    {
        public OutputParameter(string formal, Expression expression, bool inverted, LexLocation loc)
            : base(formal, expression, " => ", loc)
        {
            this.inverted = inverted;
        }

        public override bool IsOutputParameter
        {
            get { return true; }
        }

        public override Expression LValue
        {
            get;
            set;
        }

        private readonly bool inverted;
    }
}
