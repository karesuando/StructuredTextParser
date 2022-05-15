using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.Statements
{
    public class FunctionBlockCallStatement : Statement
    {
        public FunctionBlockCallStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override void GenerateCode(List<int> exitList)
        {
            this.expression.GenerateLoad();
        }

        private readonly Expression expression;
    }
}
