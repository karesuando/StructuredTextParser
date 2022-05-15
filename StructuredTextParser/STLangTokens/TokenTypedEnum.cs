using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;

namespace STLang.ConstantTokens
{
    public class TokenTypedEnum : ConstantToken
    {
        public TokenTypedEnum(string strVal)
            : base(strVal)
        {
        }

        public string TypeName { get; set; }

        public string Value { get; set; }

        public Tokens TypeToken { get; set; }

        public Tokens ValueToken { get; set; }
    }
}
