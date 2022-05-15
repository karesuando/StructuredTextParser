using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class ConversionOperator : UnaryOperator
    {
        public ConversionOperator(Expression operand, string convOp, TypeNode dataType, VirtualMachineInstruction opCode, bool isImplicitOp)
            : base(operand, convOp, dataType, opCode)
        {
            this.isImplicitOp = isImplicitOp;
        }

        public override int Priority
        {
            get { return this.isImplicitOp ? this.Operand.Priority : 9; }
        }

        public override string ToString()
        {
            if (this.isImplicitOp)
                return this.Operand.ToString();
            else
            {
                // Explicit conversion operator
                return this.Name + "(" + this.Operand + ")";
            }
        }

        private readonly bool isImplicitOp;
    }
}
