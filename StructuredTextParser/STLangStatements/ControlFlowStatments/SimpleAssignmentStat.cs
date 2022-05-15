using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ErrorManager;
using StructuredTextParser.Properties;

namespace STLang.Statements
{
    public class SimpleAssignmentStat : AssignmentStat
    {
        public SimpleAssignmentStat(Expression lValue, Expression rValue)
        {
            this.lValue = (MemoryObject)lValue;
            this.rValue = rValue;
        }

        public override void GenerateCode(List<int> exitList = null)
        {
            if (this.lValue == null)
                throw new STLangCompilerError(Resources.LVALUE);
            else if (this.rValue == null)
                throw new STLangCompilerError(Resources.RVALUE);
            else
            {
                this.rValue.GenerateLoad();
                this.lValue.GenerateStore();
            }
        }

        private readonly MemoryObject lValue;

        private readonly Expression rValue;
    }
}
