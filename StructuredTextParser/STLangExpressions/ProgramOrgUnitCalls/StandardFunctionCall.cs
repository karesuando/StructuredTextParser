using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class StandardFunctionCall : Expression
    {
        public StandardFunctionCall(Expression[] args, TypeNode dataType, StandardLibraryFunction opCode, string exprStr)
            : base(dataType, exprStr)
        {
            this.arguments = args;
            int length = 0;
            foreach (Expression arg in args)
                length += arg.Length;
            this.Length = length + 3;
            this.opCode = opCode;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            foreach (Expression argument in this.arguments)
            {
                argument.GenerateLoad();
            }
            this.StoreInstruction(this.opCode);
        }

        public override int Priority
        {
            get { return 10; }
        }
       
        protected readonly Expression[] arguments;

        private readonly StandardLibraryFunction opCode;
    }
}
