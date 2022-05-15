using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ImplDependentParams;

namespace STLang.DataTypes
{
    public class StringType : TypeNode
    {
        public StringType(string name = "STRING")
            : base(name, STLangParameters.MAX_DEFAULT_STRING_LENGTH, "T")
        {
            this.initialValue = new StringConstant("", this);
        }

        public StringType(string name, int size, string typeID)
            : base(name, (uint)size, typeID)
        {
            this.initialValue = new StringConstant("", this);
            SaveDataType(typeID, this);
        }

        public override uint Alignment
        {
            get { return Byte.Alignment; }
        }

        public override bool IsStringType 
        { 
            get { return true; } 
        }

        public override bool IsTextType
        {
            get { return true; }
        }
    }
}
