using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class UnaryOperator : Expression
    {
        public UnaryOperator(Expression expression, string op, VirtualMachineInstruction opCode)
            : base(expression.DataType, op)
        {
            this.operand = expression;
            this.opCode = opCode;
            this.Length = expression.Length + 1;
        }

        public UnaryOperator(Expression expression, string op, TypeNode dataType, VirtualMachineInstruction opCode)
            : base(dataType, op)
        {
            this.operand = expression;
            this.opCode = opCode;
            this.Length = expression.Length + 1;
        }

        public override bool IsConstant
        {
            get { return this.operand.IsConstant; }
        }

        public override bool IsCompoundExpression
        {
            get { return true; }
        }

        protected VirtualMachineInstruction OperationCode
        {
            get { return this.opCode; }
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.operand.GenerateLoad(trueBranch, falseBranch);
            this.StoreInstruction(this.opCode);
        }

        public override string ToString()
        {
            string operandStr = this.operand.ToString();
            if (this.Priority > this.operand.Priority)
                operandStr = "(" + operandStr + ")";
            return this.Name + operandStr;
        }

        public override int Priority
        {
            get { return 8; }
        }

        public Expression Operand 
        { 
            get { return this.operand; } 
        }

        private readonly Expression operand;

        private readonly VirtualMachineInstruction opCode;

    }
}
