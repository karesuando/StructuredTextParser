using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class BinaryBoolOperator : Expression
    {
        public BinaryBoolOperator(Expression left, Expression right, string stringOp)
            : base(TypeNode.Bool, stringOp)
        {
            this.operands = new List<Expression>{left, right};
        }

        public BinaryBoolOperator(List<Expression> operands, string stringOp)
            : base(TypeNode.Bool, stringOp)
        {
            this.operands = operands;
        }

        public override void GenerateLoad(List<int> trueBranch1 = null, List<int> falseBranch1 = null)
        {
            List<int> trueBranch = new List<int>();
            List<int> falseBranch = new List<int>();
            this.GenerateLoad(trueBranch, falseBranch);
            this.BackPatch(trueBranch, (uint)ILC);
            this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            this.StoreInstruction(VirtualMachineInstruction.JUMP, ILC + 2);
            this.BackPatch(falseBranch, (uint)ILC);
            this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
        }

        public abstract void Add(Expression operand);

        public abstract void AddLeft(Expression operand);

        public IEnumerable<Expression> Operands
        {
            get { return this.operands; }
        }

        public override bool IsCompoundExpression
        {
            get { return true; }
        }

        public override string ToString()
        {
            string operatorStr = base.ToString();
            string exprStr = this.operands[0].ToString();
            if (this.Priority > this.operands[0].Priority)
                exprStr = "(" + exprStr + ")";
            foreach (Expression operand in this.operands.Skip(1))
            {
                if (this.Priority <= operand.Priority)
                    exprStr += operatorStr + operand.ToString();
                else
                    exprStr += operatorStr + "(" + operand.ToString() + ")";
            }
            return exprStr;
        }

        protected readonly List<Expression> operands;
    }
}
