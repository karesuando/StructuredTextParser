using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class RelationOperator : BinaryOperator
    {
        public RelationOperator(Expression arg1, Expression arg2, string stringOp, VirtualMachineInstruction opCode, bool isZeroOp = false)
            : base(arg1, arg2, TypeNode.Bool, stringOp, opCode)
        {
            this.isZeroOp = isZeroOp;
            this.IsInverted = false;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.LeftOperand.GenerateLoad();
            if (!this.isZeroOp)
                this.RightOperand.GenerateLoad();
            if (this.IsInverted)
                trueBranch.Add(ILC);
            else
                falseBranch.Add(ILC);
            this.StoreInstruction(this.OpCode, 0);
        }

        public override Expression InvertRelation(bool doInvert)
        {
            if (!doInvert)
                return this;
            else
            {
                Expression expression = this.DeMorgan();
                expression.IsInverted = true;
                return expression;
            }
        }

        public bool IsZeroOp
        {
            get { return this.isZeroOp; }
        }

        public override bool IsInverted
        {
            get;
            set;
        }

        private readonly bool isZeroOp;
    }
}
