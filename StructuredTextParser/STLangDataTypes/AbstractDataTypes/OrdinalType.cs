using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public abstract class OrdinalType<T> : ElementaryType<T> where T : struct, IComparable<T>
    {
        public OrdinalType(string name, uint size, T lower, T upper, string typeID)
            : base(name, lower, upper, size, typeID)
        {
            this.isSubRange = false;
            this.baseType = this;
        }

        public OrdinalType(string name, uint size, T lower, T upper, TypeNode baseType, string typeID) 
            : base(name, lower, upper, size, typeID)
        {
            this.isSubRange = true;
            this.baseType = baseType;
            SaveDataType(typeID, this); 
        }

        public override TypeNode BaseType
        {
            get { return this.baseType; }
        }

        public override bool IsOrdinalType
        {
            get { return true; }
        }

        public override bool IsSubrangeType
        {
            get { return this.isSubRange; }
        }

        private readonly bool isSubRange;

        private readonly TypeNode baseType;
    }
}
