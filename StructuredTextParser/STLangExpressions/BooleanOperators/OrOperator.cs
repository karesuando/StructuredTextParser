using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class IOrOperator : BinaryBoolOperator
    {
        public IOrOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, " OR ")
        {

        }

        public IOrOperator(List<Expression> operands) 
            : base(operands, " OR ")
        {

        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            List<int> falseBranch2 = new List<int>();
            this.operands[0].GenerateBoolExpression(trueBranch, falseBranch2);
            foreach (Expression operand in this.operands.Skip(1))
            {
                uint targetAddr = (uint)ILC;
                this.BackPatch(falseBranch2, targetAddr);
                operand.GenerateBoolExpression(trueBranch, falseBranch2);
            }
            falseBranch.AddRange(falseBranch2);
        }

        public override void Add(Expression operand)
        {
            if (operand is IOrOperator)
                this.operands.AddRange(((BinaryBoolOperator)operand).Operands);
            else
                this.operands.Add(operand);
        }

        public override void AddLeft(Expression operand)
        {
            if (!(operand is IOrOperator))
                this.operands.Insert(0, operand);
            else
                this.operands.InsertRange(0, ((IOrOperator)operand).Operands);
        }

        public override Expression InvertRelation(bool doInvert)
        {
            int count = this.operands.Count - 1;
            for (int i = 0; i < count; i++)
                this.operands[i] = this.operands[i].InvertRelation(true);
            this.operands[count] = this.operands[count].InvertRelation(doInvert);
            return this;
        }

        public override Expression DeMorgan()
        {
            Expression invertedExpr;
            List<Expression> operandList = new List<Expression>();
            foreach (Expression operand in this.operands)
            {
                invertedExpr = operand.DeMorgan();
                if (invertedExpr is AndOperator)
                    operandList.AddRange(((AndOperator)invertedExpr).Operands);
                else
                    operandList.Add(invertedExpr);
            }
            return new AndOperator(operandList);
        }

        public override int Priority
        {
            get { return 1; }
        }

    }
}
