using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class RepeatStatement : Statement
    {
        public RepeatStatement(StatementList statList, Expression cond)
        {
            this.condition = cond;
            this.statementList = statList;
            this.IsFunctionValueDefined = statList.IsFunctionValueDefined;
            this.ContainsExit = statList.ContainsExit;
            this.FunctionReturns = statList.POUReturns;
            this.ControlFlowTerminates = statList.ControlFlowTerminates;
        }

        public override void GenerateCode(List<int> dummyList)
        {
            if (this.statementList == null || this.condition == null)
                throw new STLangCompilerError("Can't generate REPEAT-statement.");
            else if (this.condition.IsConstant)
            {
                 List<int> exitList = new List<int>();
                 int startLabel = ILC;
                 this.statementList.GenerateCode(exitList);
                 object value = this.condition.Evaluate();
                 bool conditionIsTrue = Convert.ToBoolean(value);
                 if (conditionIsTrue)
                     this.StoreInstruction(VirtualMachineInstruction.JUMP, startLabel);
                 uint exitLabel = (uint)ILC;
                 this.BackPatch(exitList, exitLabel);
            }
            else {
                List<int> trueBranch = new List<int>();
                List<int> falseBranch = new List<int>();
                List<int> exitList = new List<int>();
                uint startLabel = (uint)ILC;
                this.statementList.GenerateCode(exitList);
                this.condition.GenerateLoad(trueBranch, falseBranch);
                this.BackPatch(falseBranch, startLabel);
                uint exitLabel = (uint)ILC;
                this.BackPatch(trueBranch, exitLabel);
                this.BackPatch(exitList, exitLabel);
            }
        }

        private readonly Expression condition;

        private readonly StatementList statementList;
    }
}
