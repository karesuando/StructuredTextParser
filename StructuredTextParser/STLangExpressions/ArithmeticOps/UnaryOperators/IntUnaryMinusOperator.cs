using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class IntUnaryMinusOperator : UnaryOperator
    {
        public IntUnaryMinusOperator(Expression expression)
            : base(expression, "-", GetUMinInstruction(expression.DataType))
        {
        }

        public override bool IsLinear()
        {
            return this.Operand.IsLinear();
        }

        private static VirtualMachineInstruction GetUMinInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.INEG;
            else
                return VirtualMachineInstruction.LNEG;
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return -this.Operand.Evaluate(bound, value);
        }
    }
}
