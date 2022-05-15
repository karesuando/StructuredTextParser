using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ConstantTokens
{
    public class TokenDirectVar : ConstantToken
    {
        public TokenDirectVar(char size, char loc, string strVal)
            : base(strVal)
        {
            this.size = size;
            this.location = loc;
            this.address = new ushort[] { 0, 0, 0, 0 };
        }

        public TokenDirectVar(char size, char loc, ushort[] addrList, string strVal)
            : base(strVal)
        {
            this.size = size;
            this.location = loc;
            this.address = new ushort[addrList.Length];
            for (int i = 0; i < addrList.Length; i++)
            {
                this.address[i] = addrList[i];
            }
        }

        public char Size 
        { 
            get { return this.size; } 
        }

        public char Location 
        { 
            get { return this.location; } 
        }

        public ushort[] Address 
        { 
            get { return this.address; } 
        }

        private readonly char size;

        private readonly char location;

        private readonly ushort[] address;
    }
}
