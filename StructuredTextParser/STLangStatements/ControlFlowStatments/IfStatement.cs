using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class IfStatement : Statement
    {
        public IfStatement(Expression cond, StatementList thenStat, StatementList elseStat)
        {
            this.condition = cond;
            this.thenStatement = thenStat;
            this.elseStatement = elseStat;
            this.elseIfStatementList = null;
            if (cond.IsConstant)
            {
                bool conditionIsTrue = Convert.ToBoolean(cond.Evaluate());
                if (conditionIsTrue)
                {
                    this.IsFunctionValueDefined = thenStat.IsFunctionValueDefined;
                    this.ContainsExit = thenStat.ContainsExit;
                    this.FunctionReturns = thenStat.POUReturns;
                    this.ControlFlowTerminates = thenStat.ControlFlowTerminates;
                }
                else if (elseStat != null)
                {
                    this.IsFunctionValueDefined = elseStat.IsFunctionValueDefined;
                    this.ContainsExit = elseStat.ContainsExit;
                    this.FunctionReturns = elseStat.POUReturns;
                    this.ControlFlowTerminates = elseStat.ControlFlowTerminates;
                }
            }
            else if (elseStat == null)
                this.ContainsExit = thenStat.ContainsExit;
            else
            {
                this.IsFunctionValueDefined = thenStat.IsFunctionValueDefined 
                                           && elseStat.IsFunctionValueDefined;
                this.ContainsExit = thenStat.ContainsExit
                                 || elseStat.ContainsExit;
                this.FunctionReturns = thenStat.POUReturns
                                    && elseStat.POUReturns;
                this.ControlFlowTerminates = thenStat.ControlFlowTerminates
                                          && elseStat.ControlFlowTerminates;
            }
        }

        public IfStatement(Expression cond, StatementList thenStat, StatementList elseStat, List<ElseIfStatement> elseIfStatList)
        {
            this.condition = cond;
            this.thenStatement = thenStat;
            this.elseStatement = elseStat;
            this.elseIfStatementList = elseIfStatList;
            if (!cond.IsConstant)
            {
                this.IsFunctionValueDefined = thenStat.IsFunctionValueDefined;
                this.ContainsExit = thenStat.ContainsExit;
                this.FunctionReturns = thenStat.POUReturns;
                this.ControlFlowTerminates = thenStat.ControlFlowTerminates;
            }
            else {
                bool conditionIsTrue = Convert.ToBoolean(cond.Evaluate());
                if (conditionIsTrue)
                {
                    this.IsFunctionValueDefined = thenStat.IsFunctionValueDefined;
                    this.ContainsExit = thenStat.ContainsExit;
                    this.FunctionReturns = thenStat.POUReturns;
                    this.ControlFlowTerminates = thenStat.ControlFlowTerminates;
                    return;
                }
                else
                {
                    this.IsFunctionValueDefined = true;
                    this.FunctionReturns = true;
                    this.ControlFlowTerminates = true;
                }
            }
            foreach (ElseIfStatement elseIfStatement in elseIfStatList)
            {
                Expression condition = elseIfStatement.Condition;
                StatementList statList = elseIfStatement.StatementList;
                if (! condition.IsConstant)
                    this.SetProperties(statList);
                else {
                    bool conditionIsTrue = Convert.ToBoolean(condition.Evaluate());
                    if (conditionIsTrue)
                    {
                        this.SetProperties(statList);
                        return;
                    }
                }
            }
            if (elseStat != null)
                this.SetProperties(elseStat);
            else {
                this.IsFunctionValueDefined = false;
                this.FunctionReturns = false;
                this.ControlFlowTerminates = false;
            }
        }

        private void SetProperties(StatementList statList)
        {
            if (this.IsFunctionValueDefined)
                this.IsFunctionValueDefined = statList.IsFunctionValueDefined;
            if (this.FunctionReturns)
                this.FunctionReturns = statList.POUReturns;
            if (this.ControlFlowTerminates)
                this.ControlFlowTerminates = statList.ControlFlowTerminates;
            if (!this.ContainsExit)
                this.ContainsExit = statList.ContainsExit;
        }

        public override void GenerateCode(List<int> exitList)
        {
            if (this.condition == null || this.thenStatement == null)
                throw new STLangCompilerError("Can't generate IF-statement");
            else
            {
                bool controlFlowTerminates;
                List<int> trueBranch = new List<int>();
                List<int> falseBranch = new List<int>();
                List<int> jumpList = new List<int>();
                if (this.condition.IsConstant)
                {
                    bool conditionIsTrue = Convert.ToBoolean(this.condition.Evaluate());
                    if (conditionIsTrue)
                    {
                        this.thenStatement.GenerateCode(exitList);
                        return;
                    }
                }
                else
                {
                    this.condition.GenerateLoad(trueBranch, falseBranch);
                    this.BackPatch(trueBranch, (uint)ILC);
                    this.thenStatement.GenerateCode(exitList);
                    controlFlowTerminates = this.thenStatement.ControlFlowTerminates;
                    if ((this.elseIfStatementList != null || this.elseStatement != null) && !controlFlowTerminates)
                    {
                        // Generate JUMP instruction at the end of then-statement
                        jumpList.Add(ILC);
                        this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                    }
                }
                if (this.elseIfStatementList != null)
                {
                    int elseIfCount = this.elseIfStatementList.Count;
                        
                    // Generate code for elseif-statement(s)
                    foreach (ElseIfStatement elseIfStat in this.elseIfStatementList)
                    {
                        Expression condition = elseIfStat.Condition;
                        StatementList statementList = elseIfStat.StatementList;
                        if (condition == null || statementList == null)
                            throw new STLangCompilerError("Can't generate ELSEIF-statement");
                        else if (condition.IsConstant)
                        {
                            bool conditionIsTrue = Convert.ToBoolean(condition.Evaluate());
                            if (conditionIsTrue)
                            {
                                this.BackPatch(falseBranch, (uint)ILC);
                                statementList.GenerateCode(exitList);
                                this.BackPatch(jumpList, (uint)ILC);
                                return;
                            }
                            elseIfCount--;
                        }
                        else {
                            elseIfCount--;
                            this.BackPatch(falseBranch, (uint)ILC);
                            condition.GenerateLoad(trueBranch, falseBranch);
                            this.BackPatch(trueBranch, (uint)ILC);
                            statementList.GenerateCode(exitList);
                            controlFlowTerminates = statementList.ControlFlowTerminates;
                            if ((this.elseStatement != null || elseIfCount > 1) && !controlFlowTerminates)
                            { 
                                jumpList.Add(ILC);
                                this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                            }
                        }
                    }
                }
                this.BackPatch(falseBranch, (uint)ILC);
                if (this.elseStatement != null)
                {
                    // Generate code for else-statement
                    this.elseStatement.GenerateCode(exitList);
                }
                this.BackPatch(jumpList, (uint)ILC);
            }
        }

        private readonly Expression condition;

        private readonly StatementList thenStatement;

        private readonly StatementList elseStatement;

        private readonly List<ElseIfStatement> elseIfStatementList;
    }
}
