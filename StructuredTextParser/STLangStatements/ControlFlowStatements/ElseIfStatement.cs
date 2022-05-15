using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ErrorManager;

namespace STLang.Statements
{
    public class ElseIfStatement : Statement
    {
        public ElseIfStatement(Expression expression, StatementList statList)
        {
            this.condition = expression;
            this.statementList = statList;
        }

        public override void GenerateCode(List<int> exitList)
        {
            throw new NotImplementedException("ElseIf GenStatement() not implemented");
        }

        public Expression Condition
        {
            get { return this.condition; }
        }

        public StatementList StatementList
        {
            get { return this.statementList; }
        }

        private readonly Expression condition;

        private readonly StatementList statementList;
    }
}
