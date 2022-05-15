using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class ErrorType : TypeNode
    {
        public ErrorType() : base("ErrorType", 1)
        {
            this.initialValue = Expression.Error;
        }
    }
}
