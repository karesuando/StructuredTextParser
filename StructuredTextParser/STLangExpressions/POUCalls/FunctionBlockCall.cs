using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.MemoryLayout;
using STLang.ParserUtility;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class FunctionBlockCall : Expression
    {
        public FunctionBlockCall(List<Expression> arguments, TypeNode dataType, int funcBlockID, string exprStr)
            : base(TypeNode.Void, exprStr)
        {
            int length = 0;
            this.inputs = new List<Expression>();
            this.outputs = new List<Expression>();
            foreach (Expression argument in arguments)
            {
                if (argument is OutputParameter)
                    this.outputs.Add(argument);
                else
                    this.inputs.Add(argument);
                length += argument.Length + 2;
            }
            this.Length = length + 3;
            this.functionBlockID = funcBlockID;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            foreach (Expression input in this.inputs)
                input.GenerateLoad();
            this.StoreInstruction(VirtualMachineInstruction.JSBR, this.functionBlockID);
            foreach (Expression output in this.outputs)
                output.GenerateStore();
        }

        private readonly int functionBlockID;

        private readonly List<Expression> inputs;

        private readonly List<Expression> outputs;
    }
}
