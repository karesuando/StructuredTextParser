using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.ImplDependentParams;
using STLang.Expressions;

namespace STLang.DataTypes
{
    public class WStringType : TypeNode
    {
        public WStringType(string name = "WSTRING")
            : base(name, STLangParameters.MAX_DEFAULT_WSTRING_LENGTH, "A")
        {
            this.initialValue = new WStringConstant("", this);
        }

        public WStringType(string name, int size, string typeID)
            : base(name, (uint)size, typeID)
        {
            this.initialValue = new WStringConstant("", this);
            SaveDataType(typeID, this);
        }

        public override uint Alignment
        {
            get { return TypeNode.UInt.Alignment; }
        }

        public override bool IsWStringType 
        { 
            get { return true; } 
        }

        public override bool IsTextType
        {
            get { return true; }
        }
    }
}
