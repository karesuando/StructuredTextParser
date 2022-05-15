using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Statements
{
    class FunctionResultStatement : Statement
    {
        public FunctionResultStatement(Expression result)
        {
            this.result = result;
            this.IsFunctionValueDefined = true;
        }

        public override void GenerateCode(List<int> exitList)
        {
            if (result == null || result.DataType == TypeNode.Error)
                throw new STLangCompilerError("Can't generate function result.");
            else if (this.result.DataType != TypeNode.Bool)
                this.result.GenerateLoad();
            else if (this.result.IsConstant || this.result is MemoryObject)
                this.result.GenerateLoad();
            else
            {
                List<int> trueBranch = new List<int>();
                List<int> falseBranch = new List<int>();
                this.result.GenerateLoad(trueBranch, falseBranch);
                this.BackPatch(trueBranch, (uint)ILC);
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
                this.StoreInstruction(VirtualMachineInstruction.JUMP, ILC + 1);
                this.BackPatch(falseBranch, (uint)ILC);
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            }
        }

        private readonly Expression result;
    }
}
