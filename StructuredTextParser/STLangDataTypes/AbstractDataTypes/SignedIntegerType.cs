using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public abstract class SignedIntegerType<T> : OrdinalType<T> where T : struct, IComparable<T>
    {
        public SignedIntegerType(string name, uint size, T lower, T upper, string typeID) 
            : base(name, size, lower, upper, typeID)
        {

        }

        public SignedIntegerType(string name, uint size, T lower, T upper, TypeNode baseType, string typeID)
            : base(name, size, lower, upper, baseType, typeID)
        {

        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else if (! expression.DataType.IsSignedIntType)
                return MAX_CONVERSION_COST;
            else if (expression.DataType == this)
                return 0.0f;
            else if (this.Size >= expression.DataType.Size)
                return 0.333f;
            else
                return 0.333f*(expression.DataType.Size / this.Size);
        }

        public override bool IsIntegerType 
        { 
            get { return true; } 
        }

        public override bool IsSignedIntType 
        { 
            get { return true; } 
        }

        public override bool IsNumericalType 
        { 
            get { return true; } 
        }
    }
}
