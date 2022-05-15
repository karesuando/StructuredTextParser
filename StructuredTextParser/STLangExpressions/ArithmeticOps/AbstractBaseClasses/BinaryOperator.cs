using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Statements;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class BinaryOperator : Expression
    {
        public BinaryOperator(Expression left, Expression right, TypeNode dataType, string op, VirtualMachineInstruction opCode, bool zeroOp = false)
            : base(dataType, op)
        {
            this.leftOperand = left;
            this.rightOperand = right;
            this.Length = left.Length + right.Length + 1;
            this.opCode = opCode;
        }

        public override bool IsConstant
        {
            get
            {
                return this.leftOperand.IsConstant && this.rightOperand.IsConstant;
            }
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.leftOperand.GenerateLoad(trueBranch, falseBranch);
            this.rightOperand.GenerateLoad(trueBranch, falseBranch);
            this.StoreInstruction(this.opCode);
        }

        public Expression LeftOperand 
        { 
            get { return this.leftOperand; } 
        }

        public Expression RightOperand 
        { 
            get { return this.rightOperand; } 
        }

        public VirtualMachineInstruction OpCode 
        { 
            get { return this.opCode; } 
        }

        public override bool IsCompoundExpression
        {
            get { return true; }
        }

        public override bool ConstantForLoopBounds(List<ForLoopData> forLoopDataList)
        {
            return this.LeftOperand.ConstantForLoopBounds(forLoopDataList)
                && this.RightOperand.ConstantForLoopBounds(forLoopDataList);
        }

        public override string ToString()
        {
            string lExprStr = this.leftOperand.ToString();
            if (this.Priority > this.leftOperand.Priority)
                lExprStr = "(" + lExprStr + ")";
            string rExprStr = this.rightOperand.ToString();
            if (this.Priority > this.rightOperand.Priority)
                rExprStr = "(" + rExprStr + ")";
            return lExprStr + this.Name + rExprStr;
        }

        private readonly Expression leftOperand;

        private readonly Expression rightOperand;

        private readonly VirtualMachineInstruction opCode;
    }
}
