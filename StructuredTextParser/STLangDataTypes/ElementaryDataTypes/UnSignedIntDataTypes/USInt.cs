using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class USIntType : UnsignedIntegerType<byte>
    {
        public USIntType(string name = "USINT")
            : base(name, USINT_SIZE, byte.MinValue, byte.MaxValue, "U")
        {
            this.initialValue = new USIntConstant(0, this, name + "#0");
        }

        public USIntType(string name, byte lowerBound, byte upperBound, TypeNode baseType, string typeID)
            : base(name, USINT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound;
            this.initialValue = new USIntConstant(lowerBound, this, strValue);
        }

        public USIntType(string name, byte lowerBound, byte upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, USINT_SIZE, lowerBound, upperBound, baseType, typeID)
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
                return USInt;
            }
            else if (subRange is IntSubrange)
            {
                IntSubrange intSubRange = (IntSubrange)subRange;
                byte lower = (byte)intSubRange.LowerBound;
                byte upper = (byte)intSubRange.UpperBound;
                string typeID = USInt.TypeID + "(" + subRange + ")";
                return new USIntType(name, lower, upper, this, typeID);
            }
            else
            {
                UIntSubrange uintSubRange = (UIntSubrange)subRange;
                byte lower = (byte)uintSubRange.LowerBound;
                byte upper = (byte)uintSubRange.UpperBound;
                string typeID = USInt.TypeID + "(" + subRange + ")";
                return new USIntType(name, lower, upper, this, typeID);
            }
        }
    }
}
