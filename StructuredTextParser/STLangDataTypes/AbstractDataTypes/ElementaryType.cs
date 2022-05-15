using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public abstract class ElementaryType<T> : TypeNode where T : struct, IComparable<T>
    {
        public ElementaryType(string name, T lower, T upper, uint size,  string typeID)
            : base(name, size, typeID)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
        }

        public override bool IsElementaryType 
        { 
            get { return true; } 
        }

        public T LowerBound
        {
            get { return this.lowerBound; }
        }

        public T UpperBound
        {
            get { return this.upperBound; }
        }

        private readonly T lowerBound;

        private readonly T upperBound;
    }
}
