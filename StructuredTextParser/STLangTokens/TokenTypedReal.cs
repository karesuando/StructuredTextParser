using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;

namespace STLang.ConstantTokens
{
    public class TokenTypedReal : ConstantToken
    {
        public TokenTypedReal(Tokens token, double realValue, string typeName, string strValue)
            : base(strValue)
        {
            this.realType = token;
            this.realValue = realValue;
            this.typeName = typeName;
        }

        public double Value 
        { 
            get { return this.realValue; } 
        }

        public Tokens RealType 
        { 
            get { return this.realType; } 
        }

        public string TypeName 
        { 
            get { return this.typeName; } 
        }

        private readonly Tokens realType;

        private readonly double realValue;

        private readonly string typeName;
    }
}
