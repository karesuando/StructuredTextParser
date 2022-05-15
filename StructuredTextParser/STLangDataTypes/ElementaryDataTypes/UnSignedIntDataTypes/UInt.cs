using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class UIntType : UnsignedIntegerType<ushort>
    {
        public UIntType(string name = "UINT")
            : base(name, UINT_SIZE, ushort.MinValue, ushort.MaxValue, "Y")
        {
            this.initialValue = new UIntConstant(0, this, name + "#0");
        }

        public UIntType(string name, ushort lowerBound, ushort upperBound, TypeNode baseType, string typeID)
            : base(name, UINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound;
            this.initialValue = new UIntConstant(lowerBound, TypeNode.UInt, strValue);
        }

        public UIntType(string name, ushort lowerBound, ushort upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, UINT_SIZE, lowerBound, upperBound, baseType, typeID)
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
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return UInt;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                ushort lower = (ushort)intSubrange.LowerBound;
                ushort upper = (ushort)intSubrange.UpperBound;
                string typeID = UDInt.TypeID + "(" + subRange + ")";
                return new UIntType(name, lower, upper, this, typeID);
            }
            else
            {
                UIntSubrange uintSubrange = (UIntSubrange)subRange;
                ushort lower = (ushort)uintSubrange.LowerBound;
                ushort upper = (ushort)uintSubrange.UpperBound;
                string typeID = UDInt.TypeID + "(" + subRange + ")";
                return new UIntType(name, lower, upper, this, typeID);
            }
        }
    }
}
