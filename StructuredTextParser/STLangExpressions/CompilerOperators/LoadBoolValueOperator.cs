using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LoadBoolValueOperator : Expression
    {
        public LoadBoolValueOperator(Expression operand)
            : base(operand.DataType, operand.ToString())
        {
            if (operand.DataType != TypeNode.Bool)
            {
                string msg;
                msg = "LoadBoolValueOperator: Boolean expression expected.";
                throw new STLangCompilerError(msg);
            }
            else if (operand.IsConstant || operand.IsLValue)
            {
                string msg;
                msg = "LoadBoolValueOperator: Compound expression expected.";
                throw new STLangCompilerError(msg);
            }
            this.operand = operand.InvertRelation();
        }

        public override void GenerateLoad(List<int> trueBranch1 = null, List<int> falseBranch1 = null)
        {
            List<int> trueBranch = new List<int>();
            List<int> falseBranch = new List<int>();
            this.operand.GenerateLoad(trueBranch, falseBranch);
            BackPatch(trueBranch, (uint)ILC);
            this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            this.StoreInstruction(VirtualMachineInstruction.JUMP, ILC + 2);
            BackPatch(falseBranch, (uint)ILC);
            this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
        }

        private readonly Expression operand;
    }
}
