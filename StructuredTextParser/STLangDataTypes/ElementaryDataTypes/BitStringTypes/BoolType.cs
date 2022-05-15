using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Subranges;
using STLang.Expressions;
using QUT.Gppg;

namespace STLang.DataTypes
{
    public class BoolType : BitStringType<bool>
    {
        public BoolType()
            : base("BOOL", BOOL_SIZE, false, true, "B")
        {
            this.initialValue = new BoolConstant(false, this, "FALSE");
        }

        public override TypeNode MakeSubrange(string name, SubRange subRange, LexLocation loc)
        {
            return Error;
        }

        public override bool IsInRange(SubRange subRange)
        {
            return true;
        }

        public override int BitCount 
        { 
            get { return 1; } 
        }
    }
}
