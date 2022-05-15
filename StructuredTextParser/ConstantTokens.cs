using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public abstract class ConstantToken
    {
        public ConstantToken(string strVal)
        {
            this.stringVal = strVal;
        }

        public override string ToString()
        {
            return this.stringVal;
        }

        private readonly string stringVal;
    }
}
