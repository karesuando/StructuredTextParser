using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenInt : ConstantToken
    {
        public TokenInt(ulong value, string strVal)
            : base(strVal)
        {
            this.value = value;
        }

        public ulong Value 
        { 
            get { return this.value; } 
        }

        private readonly ulong value;
    }
}
