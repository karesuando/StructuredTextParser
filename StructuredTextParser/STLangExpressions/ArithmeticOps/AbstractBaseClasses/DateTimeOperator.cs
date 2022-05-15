using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class DateTimeOperator : BinaryOperator
    {
        public DateTimeOperator(Expression left, Expression right, TypeNode dataType, string expr, StandardLibraryFunction stdFunc)
            : base(left, right, dataType, expr, VirtualMachineInstruction.NOOP)
        {
            this.stdFunction = stdFunc;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.LeftOperand.GenerateLoad(trueBranch, falseBranch);
            this.RightOperand.GenerateLoad(trueBranch, falseBranch);
            this.StoreInstruction(this.stdFunction);
        }

        private readonly StandardLibraryFunction stdFunction;
    }
}
