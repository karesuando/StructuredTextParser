using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class BitAndOperator : BinaryOperator
    {
        public BitAndOperator(Expression arg1, Expression arg2, TypeNode dataType) 
            : base(arg1, arg2, dataType, " AND ", VirtualMachineInstruction.IBAND)
        {

        }
    }
}
