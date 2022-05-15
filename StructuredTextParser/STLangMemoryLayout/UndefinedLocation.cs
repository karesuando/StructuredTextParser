using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;

namespace STLang.MemoryLayout
{
    public class UndefinedLocation : MemoryLocation
    {
        public UndefinedLocation() : base(Expression.UNDEFINED_INDEX, TypeNode.Error, 1)
        {

        }

        public override MemoryLocation AddOffset(int offset)
        {
            return this;
        }
    }
}
