using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenDouble : ConstantToken
    {
        public TokenDouble(double value, string strVal)
            : base(strVal)
        {
            this.value = value;
        }

        public double Value 
        { 
            get { return this.value; } 
        }

        private readonly double value;
    }
}
