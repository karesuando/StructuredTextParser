using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class IntDivOperator : BinaryOperator
    {
        public IntDivOperator(Expression arg1, Expression arg2, TypeNode dataType)
            : base(arg1, arg2, dataType, "/", GetDivInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetDivInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IDIV;
            else
                return VirtualMachineInstruction.LDIV;
        }

        public override bool IsLinear()
        {
            return this.LeftOperand.IsLinear() && this.RightOperand.IsConstant;
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            int indexValue = this.RightOperand.Evaluate(bound, value);
            if (indexValue == 0)
                return 0;
            else
                return this.LeftOperand.Evaluate(bound, value) / indexValue;
        }

        public override int Priority
        {
            get { return 7; }
        }
    }
}
