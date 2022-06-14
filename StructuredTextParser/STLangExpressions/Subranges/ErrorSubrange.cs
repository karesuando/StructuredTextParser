using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.Subranges
{
    public class ErrorSubrange : SubRange
    {
        public ErrorSubrange()
            : base(Expression.Error, Expression.Error, TypeNode.Error)
        {
        }

        public override bool Contains(SubRange subRange)
        {
            throw new NotImplementedException("Compiler Error: Method ErrorSubRange.Contains(SubRange) not implemented.");
        }

        public override bool Contains(Expression value)
        {
            throw new NotImplementedException("Compiler Error: Method ErrorSubRange.Contains(Expression) not implemented.");
        }

        public override bool IsDisjoint(SubRange subRange)
        {
            throw new NotImplementedException("Compiler Error: Method ErrorSubRange.IsDisjoint(SubRange) not implemented.");
        }

        public override object CreateInterval()
        {
            throw new NotImplementedException("Compiler Error: Method ErrorSubRange.CreateInterval() not implemented.");
        }
    }
}
