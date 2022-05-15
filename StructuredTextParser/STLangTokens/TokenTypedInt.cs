using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;

namespace STLang.ConstantTokens
{
    public class TokenTypedInt : ConstantToken
    {
        public TokenTypedInt(Tokens token, ulong intValue, string typeName, string strValue)
            : base(strValue)
        {
            this.intType = token;
            this.value = intValue;
            this.typeName = typeName;
        }

        public ulong Value 
        { 
            get { return this.value; } 
        }

        public Tokens IntType 
        { 
            get { return this.intType; } 
        }

        public string TypeName 
        { 
            get { return this.typeName; } 
        }

        private readonly Tokens intType;

        private readonly ulong value;

        private readonly string typeName;
    }
}
