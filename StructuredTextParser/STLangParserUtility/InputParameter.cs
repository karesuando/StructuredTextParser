using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.ParserUtility
{
    public class InputParameter : POUParameter
    {
        public InputParameter(Expression expression, LexLocation location)
            : base(expression, location)
        {
            this.isInputArgAssign = false;
        }

        public InputParameter(string formal, Expression expression, LexLocation location)
            : base(formal, expression, " := ", location)
        {
            this.isInputArgAssign = true;
        }

        public override bool IsInputParameter
        {
            get { return true; }
        }

        public override bool IsConstant
        {
            get { return this.RValue.IsConstant; }
        }

        public override object Evaluate()
        {
            return this.RValue.Evaluate();
        }

        public override bool IsInputArgAssignment
        {
            get { return this.isInputArgAssign; }
        }

        private readonly bool isInputArgAssign;
    }
}
