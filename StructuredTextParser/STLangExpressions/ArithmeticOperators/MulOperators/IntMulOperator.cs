using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class IntMulOperator : BinaryOperator
    {
        public IntMulOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, "*", GetMulInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetMulInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IMUL;
            else
                return VirtualMachineInstruction.LMUL;
        }

        public override bool IsLinear()
        {
            return this.LeftOperand.IsConstant && this.RightOperand.IsLinear()
                || this.LeftOperand.IsLinear() && this.RightOperand.IsConstant;
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return this.LeftOperand.Evaluate(bound, value)
                 * this.RightOperand.Evaluate(bound, value);
        }

        public override int Priority
        {
            get { return 7; }
        }
    }
}
