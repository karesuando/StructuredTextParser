using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class WhileStatement : Statement
    {
        public WhileStatement(Expression cond, StatementList statList)
        {
            this.condition = cond;
            this.statementList = statList;
            if (!cond.IsConstant)
                this.ContainsExit = statList.ContainsExit;
            else
            {
                bool conditionIsTrue = Convert.ToBoolean(cond.Evaluate());
                if (conditionIsTrue)
                {
                    this.IsFunctionValueDefined = statList.IsFunctionValueDefined;
                    this.ContainsExit = statList.ContainsExit;
                    this.FunctionReturns = statList.POUReturns;
                    this.ControlFlowTerminates = statList.ControlFlowTerminates;
                }
            }
        }

        public override void GenerateCode(List<int> dummyList)
        {
            if (this.statementList == null || this.condition == null)
                throw new STLangCompilerError("Can't to generate WHILE-statement.");
            else if (this.condition.IsConstant)
            {
                bool conditionIsTrue = Convert.ToBoolean(this.condition.Evaluate());

                if (conditionIsTrue)
                {
                    int startLabel = ILC;
                    List<int> exitList = new List<int>();
                    this.statementList.GenerateCode(exitList);
                    this.StoreInstruction(VirtualMachineInstruction.JUMP, startLabel);
                    uint exitLabel = (uint)ILC;
                    this.BackPatch(exitList, exitLabel);
                }
            }
            else {
                List<int> trueBranch = new List<int>();
                List<int> falseBranch = new List<int>();
                List<int> exitList = new List<int>();
                int whileLabel = ILC;
                this.condition.GenerateLoad(trueBranch, falseBranch);
                this.BackPatch(trueBranch, (uint)ILC);
                this.statementList.GenerateCode(exitList);
                this.StoreInstruction(VirtualMachineInstruction.JUMP, whileLabel);
                uint exitLabel = (uint)ILC;
                this.BackPatch(falseBranch, exitLabel);
                this.BackPatch(exitList, exitLabel);
            }
        }

        private readonly Expression condition;

        private readonly StatementList statementList;
    }
}
