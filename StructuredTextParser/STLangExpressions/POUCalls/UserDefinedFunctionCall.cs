using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ParserUtility;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    class UserDefinedFunctionCall : Expression
    {
        public UserDefinedFunctionCall(Expression[] arguments, TypeNode dataType, int funcID, string exprStr, STLangSymbol symbol)
            : base(dataType, exprStr)
        {
            int length = 0; 
            this.inputs = new List<Expression>();
            this.outputs = new List<Expression>();
            foreach (Expression argument in arguments)
            {
                if (argument is OutputParameter)
                {
                    length += argument.Length + 2;
                    this.outputs.Add(argument);
                }
                else
                {
                    length += argument.Length;
                    this.inputs.Add(argument);
                }
            }
            this.Length = length + 3; 
            this.functionID = funcID;
            this.userDefFunctionSymbol = (UserDefinedFunctionSymbol)symbol;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            foreach (Expression input in this.inputs)
                input.GenerateLoad();
            this.StoreInstruction(VirtualMachineInstruction.JSBR, this.functionID);
            foreach (Expression output in this.outputs)
                output.GenerateStore();
        }

        public override int Priority
        {
            get { return 10; }
        }

        public STLangSymbol UserDefinedFunctionSymbol
        {
            get { return this.userDefFunctionSymbol; }
        }

        private readonly int functionID;

        private readonly UserDefinedFunctionSymbol userDefFunctionSymbol;

        private readonly List<Expression> inputs;

        private readonly List<Expression> outputs;
    }
}
