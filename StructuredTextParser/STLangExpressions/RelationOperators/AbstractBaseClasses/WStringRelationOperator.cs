using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class WStringRelationOperator : RelationOperator
    {
        public WStringRelationOperator(Expression arg1, Expression arg2, string stringOp, VirtualMachineInstruction opCode)
            : base(arg1, arg2, stringOp, opCode)
        {
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.LeftOperand.GenerateLoad();
            this.RightOperand.GenerateLoad();
            this.StoreInstruction(StandardLibraryFunction.WSTRCMP);
            if (this.IsInverted)
                trueBranch.Add(ILC);
            else
                falseBranch.Add(ILC);
            this.StoreInstruction(this.OpCode, 0);
        }
    }
}