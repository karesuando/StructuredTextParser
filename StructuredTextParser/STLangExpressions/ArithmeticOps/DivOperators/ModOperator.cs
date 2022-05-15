using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class ModOperator : BinaryOperator
    {
        public ModOperator(Expression arg1, Expression arg2) 
            : base(arg1, arg2, arg2.DataType, " MOD ", VirtualMachineInstruction.IMOD)
        {

        }

        public override int Priority
        {
            get { return 7; }
        }
    }
}
