using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.MemoryLayout
{
    public class RegisterVariable : MemoryLocation
    {
        public RegisterVariable(int index, TypeNode dataType) :
            base(index, dataType, 1)
        {
        }

        public override bool IsRegisterVariable
        {
            get { return true; }
        }
    }
}
