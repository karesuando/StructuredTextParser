using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class UDIntType : UnsignedIntegerType<uint>
    {
        public UDIntType(string name = "UDINT")
            : base(name, UDINT_SIZE, uint.MinValue, uint.MaxValue, "K")
        {
            this.initialValue = new UDIntConstant(0, this, name + "#0");
        }

        public UDIntType(string name, uint lowerBound, uint upperBound, TypeNode baseType, string typeID)
            : base(name, UDINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound;
            this.initialValue = new UDIntConstant(lowerBound, this, strValue);
        }

        public UDIntType(string name, uint lowerBound, uint upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, UDINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = initVal;
        }

        public override bool IsInRange(ulong value)
        {
            return value >= this.LowerBound && value <= this.UpperBound;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return subRange.InRange(this.LowerBound, this.UpperBound);
        }

        public override SubRange GetSubrange()
        {
            return new UIntSubrange(this.LowerBound, this.UpperBound, this);
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            if (! subRange.DataType.IsIntegerType)
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (! this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return UDInt;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                uint lower = (uint)intSubrange.LowerBound;
                uint upper = (uint)intSubrange.UpperBound;
                string typeID = UDInt.TypeID + "(" + subRange + ")";
                return new UDIntType(name, lower, upper, this, typeID);
            }
            else
            {
                UIntSubrange uintSubrange = (UIntSubrange)subRange;
                uint lower = (uint)uintSubrange.LowerBound;
                uint upper = (uint)uintSubrange.UpperBound;
                string typeID = UDInt.TypeID + "(" + subRange + ")";
                return new UDIntType(name, lower, upper, this, typeID);
            }
        }
    }
}
