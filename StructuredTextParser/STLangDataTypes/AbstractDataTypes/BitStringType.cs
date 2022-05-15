using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public abstract class BitStringType<T> : OrdinalType<T> where T : struct, IComparable<T>
    {
        public BitStringType(string name, uint size, T lower, T upper, string typeID)
            : base(name, size, lower, upper, typeID)
        {

        }

        public BitStringType(string name, uint size, T lower, T upper, TypeNode baseType, string typeID)
            : base(name, size, lower, upper, baseType, typeID)
        {

        }

        public override float ConversionCost(Expression expression)
        {
            if (expression == null)
                return 0.0f;
            else if (expression.DataType == Error)
                return 0.0f;
            else if (expression.IsConstant && expression.DataType.IsIntegerType)
            {
                // conversion from (unsigned) integer to bit string
                if (this.Size >= expression.DataType.Size)
                    return 1.0f;
                else
                    return 1.0f * (expression.DataType.Size / this.Size);
            }
            else if (!expression.DataType.IsBitStringType)
                return MAX_CONVERSION_COST;
            else if (expression.DataType == this)
                return 0.0f;
            else if (this.Size >= expression.DataType.Size)
                return 0.333f;   // promotion
            else
                return 0.333f * (expression.DataType.Size / this.Size);
        }

        public override bool IsBitStringType
        {
            get { return true; }
        }
    }
}