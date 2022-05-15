using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RotateLeftOperator : Expression
    {
        public RotateLeftOperator(Expression expression, int bits)
            : base(expression.DataType, expression.ToString())
        {
            this.bits = bits;
            this.operand = expression;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            this.operand.GenerateLoad();
            this.StoreInstruction(VirtualMachineInstruction.BROL, this.bits);
        }

        private readonly int bits;

        private readonly Expression operand;
    }
}
