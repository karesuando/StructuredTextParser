using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class AndOperator : BinaryBoolOperator
    {
        public AndOperator(Expression arg1, Expression arg2) 
            : base(arg1, arg2,  " AND ")
        {

        }

        public AndOperator(List<Expression> operands) 
            : base(operands, " AND ")
        {

        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            List<int> trueBranch2 = new List<int>();
            this.operands[0].GenerateBoolExpression(trueBranch2, falseBranch);
            foreach (Expression operand in this.operands.Skip(1))
            {
                uint targetAddr = (uint)ILC;
                this.BackPatch(trueBranch2, targetAddr);
                operand.GenerateBoolExpression(trueBranch2, falseBranch);
            }
            trueBranch.AddRange(trueBranch2);
        }

        public override Expression InvertRelation(bool doInvert)
        {
            int count = this.operands.Count - 1;
            for (int i = 0; i < count; i++)
                this.operands[i] = this.operands[i].InvertRelation();
            this.operands[count] = this.operands[count].InvertRelation(doInvert);
            return this;
        }

        public override void Add(Expression operand)
        {
            if (operand is AndOperator)
                this.operands.AddRange(((BinaryBoolOperator)operand).Operands);
            else
                this.operands.Add(operand);
        }

        public override void AddLeft(Expression operand)
        {
            if (!(operand is AndOperator))
                this.operands.Insert(0, operand);
            else
                this.operands.InsertRange(0, ((AndOperator)operand).Operands);
        }

        public override Expression DeMorgan()
        {
            Expression invertedExpr;
            List<Expression> operandList = new List<Expression>();
            foreach (Expression operand in this.operands)
            {
                invertedExpr = operand.DeMorgan();
                if (invertedExpr is IOrOperator)
                    operandList.AddRange(((IOrOperator)invertedExpr).Operands);
                else
                    operandList.Add(invertedExpr);
            }
            return new IOrOperator(operandList);
        }

        public override int Priority
        {
            get { return 3; }
        }
    }
}
