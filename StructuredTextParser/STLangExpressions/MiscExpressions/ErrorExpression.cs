using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;

namespace STLang.Expressions
{
    public class ErrorExpression : Expression
    {
        public ErrorExpression()
            : base(null, "#ERROR#")
        {
        }

        public override TypeNode DataType 
        { 
            get { return TypeNode.Error; } 
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            throw new NotImplementedException("ErrorExpression.GenerateLoad() not implemented");
        }
    }
}
