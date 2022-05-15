using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;

namespace STLang.MemoryLayout
{
    public class RegisterConstant : MemoryLocation
    {
        public RegisterConstant(TypeNode dataType, int index) :
            base(index, dataType, 1)
        {
        }

        public override bool IsRegisterConstant
        {
            get { return true; }
        }
    }
}
