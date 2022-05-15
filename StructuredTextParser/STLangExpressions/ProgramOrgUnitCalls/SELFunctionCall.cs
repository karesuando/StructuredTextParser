using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class SelectFunctionCall : StandardFunctionCall
    {
        public SelectFunctionCall(Expression[] args, TypeNode dataType, string exprStr)
            : base(args, dataType, StandardLibraryFunction.NONE, exprStr)
        {

        }

        public override void GenerateLoad(List<int> trueBr, List<int> falseBr)
        {
            List<int> trueBranch = new List<int>();
            List<int> falseBranch = new List<int>();
            Expression gate = this.arguments[0];
            Expression firstInput = this.arguments[1];
            Expression secondInput = this.arguments[2];
            gate.GenerateBoolExpression(trueBranch, falseBranch);
            this.BackPatch(trueBranch, (uint)ILC);
            firstInput.GenerateLoad();
            int label = ILC;
            this.StoreInstruction(VirtualMachineInstruction.JUMP);
            this.BackPatch(falseBranch, (uint)ILC);
            secondInput.GenerateLoad();
            this.BackPatch(label, ILC);
        }
    }
}
