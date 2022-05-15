using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RangeCheckOperator : UnaryOperator
    {
        public RangeCheckOperator(Expression operand, Expression lower, Expression upper)
            : base(operand, "><", operand.DataType, VirtualMachineInstruction.RANGE_CHECK)
        {
            this.lowerBound = lower;
            this.upperBound = upper;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            // Don't pop value of array index from stack
            this.Operand.GenerateLoad();
            this.upperBound.GenerateLoad();
            this.lowerBound.GenerateLoad();
            this.StoreInstruction(VirtualMachineInstruction.RANGE_CHECK);
        }

        public override bool IsConstant
        {
            get { return this.Operand.IsConstant; }
        }

        public override bool IsLValue
        {
            get { return this.Operand.IsLValue; }
        }

        public override int Priority
        {
            get { return this.Operand.Priority; }
        }

        public override bool IsLinear()
        {
            return this.Operand.IsLinear();
        }

        public override string ToString()
        {
            // Don't show implicit operators
            return this.Operand.ToString();
        }

        private readonly Expression lowerBound;

        private readonly Expression upperBound;
    }
}
