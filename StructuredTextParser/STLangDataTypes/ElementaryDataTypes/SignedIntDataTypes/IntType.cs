using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class IntType : SignedIntegerType<short>
    {
        public IntType(string name = "INT")
            : base(name, INT_SIZE, short.MinValue, short.MaxValue, "I")
        {
            this.initialValue = new IntConstant(0, this, name + "#0");
        }

        public IntType(string name, short lowerBound, short upperBound, TypeNode baseType, string typeID)
            : base(name, INT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            string strValue = name + "#" + lowerBound;
            this.initialValue = new IntConstant(lowerBound, TypeNode.Int, strValue);
        }

        public IntType(string name, short lowerBound, short upperBound, Expression initVal, TypeNode baseType, string typeID)
            : base(name, INT_SIZE, lowerBound, upperBound, baseType, typeID)
        {
            this.initialValue = initVal;
        }

        public override bool IsInRange(long value)
        {
            return value >= this.LowerBound && value <= this.UpperBound;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return subRange.InRange(this.LowerBound, this.UpperBound);
        }

        public override SubRange GetSubrange()
        {
            return new IntSubrange(this.LowerBound, this.UpperBound, this);
        }

        public override TypeNode BaseType
        {
            get { return TypeNode.Int; }
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            if (! (subRange is IntSubrange))
            {
                Report.SemanticError(-2, subRange.ToString(), this.Name, loc);
                return Error;
            }
            else if (!this.IsInRange(subRange))
            {
                Report.SemanticError(-3, subRange.ToString(), this.Name, loc);
                return Int;
            }
            else
            {
                IntSubrange intSubrange = (IntSubrange)subRange;
                short lower = (short)intSubrange.LowerBound;
                short upper = (short)intSubrange.UpperBound;
                string typeID = Int.TypeID + "(" + subRange + ")";
                return new IntType(name, lower, upper, this, typeID);
            }
        }
    }
}
