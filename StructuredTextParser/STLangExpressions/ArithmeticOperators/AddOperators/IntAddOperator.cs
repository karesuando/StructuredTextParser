using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class IntAddOperator : BinaryOperator
    {
        public IntAddOperator(Expression arg1, Expression arg2, TypeNode dataType) 
            : base(arg1, arg2, dataType, " + ", GetAddInstruction(dataType))
        {

        }

        private static VirtualMachineInstruction GetAddInstruction(TypeNode dataType)
        {
            if (dataType.Size < TypeNode.LInt.Size)
                return VirtualMachineInstruction.IADD;
            else
                return VirtualMachineInstruction.LADD;
        }

        public override int Priority
        {
            get { return 6; }
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return this.LeftOperand.Evaluate(bound, value)
                 + this.RightOperand.Evaluate(bound, value);
        }

        public override bool IsLinear()
        {
            return this.LeftOperand.IsLinear() && this.RightOperand.IsLinear();
        }
    }
}
