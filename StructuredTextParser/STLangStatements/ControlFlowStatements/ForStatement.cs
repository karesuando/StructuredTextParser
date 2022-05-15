using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class ForStatement : Statement
    {
        public ForStatement(MemoryObject controlVar, Expression initValue, Expression condition, Expression increment, StatementList statList, bool executeLoopOnce = false)
        {
            this.controlVar = controlVar;
            this.initialValue = initValue;
            this.stopCondition = condition;
            this.increment = increment;
            this.statementList = statList;
            this.temporary = null;
            this.compoundStopValue = null;
            if (executeLoopOnce)
            {
                this.IsFunctionValueDefined = statList.IsFunctionValueDefined;
                this.FunctionReturns = statList.POUReturns;
                this.ControlFlowTerminates = this.FunctionReturns;
            }
        }

        public ForStatement(MemoryObject controlVar, Expression initValue, Expression stopValue, MemoryObject temporary, Expression condition, Expression increment, StatementList statList)
        {
            this.controlVar = controlVar;
            this.initialValue = initValue;
            this.stopCondition = condition;
            this.increment = increment;
            this.statementList = statList;
            this.temporary = temporary;
            this.compoundStopValue = stopValue;
        }

        public override void GenerateCode(List<int> dummyList)
        {
            if (this.controlVar == null || this.initialValue == null 
             || this.stopCondition == null || this.statementList == null)
                throw new STLangCompilerError("Can't generate FOR-statement");
            else
            {
                List<int> exitList = new List<int>();
                List<int> trueBranch = new List<int>();
                List<int> falseBranch = new List<int>();
                
                this.initialValue.GenerateLoad();
                this.controlVar.GenerateStore();
                if (this.compoundStopValue != null && this.temporary != null)
                {
                    // The stop value is a compound expression whose value does
                    // not change inside the loop. Evaluate the expression once
                    // and save it in a temporary.
                    this.compoundStopValue.GenerateLoad();
                    this.temporary.GenerateStore();
                }
                int startLabel = ILC;
                this.stopCondition.GenerateLoad(trueBranch, falseBranch);
                this.statementList.GenerateCode(exitList);
                this.GenerateCtrlVarIncrement();
                this.StoreInstruction(VirtualMachineInstruction.JUMP, startLabel);
                uint exitLabel = (uint)ILC;
                this.BackPatch(exitList, exitLabel);
                this.BackPatch(falseBranch, exitLabel);
            }
        }

        private void GenerateCtrlVarIncrement()
        {
            if (!this.increment.IsConstant)
            {
                this.increment.GenerateLoad();
                this.controlVar.GenerateLoad();
                this.StoreInstruction(VirtualMachineInstruction.IADD);
                this.controlVar.GenerateStore();
            }
            else {
                int incr = Convert.ToInt32(this.increment.Evaluate());
                if (incr == 1)
                {
                    int index = this.controlVar.Location.Index;
                    if (this.controlVar.DataType == TypeNode.Int)
                    {
                        if (index == 0)
                            this.StoreInstruction(VirtualMachineInstruction.IINCR0);
                        else if (index == 1)
                            this.StoreInstruction(VirtualMachineInstruction.IINCR1);
                        else if (index == 2)
                            this.StoreInstruction(VirtualMachineInstruction.IINCR2);
                        else if (index == 3)
                            this.StoreInstruction(VirtualMachineInstruction.IINCR3);
                        else
                        {
                            this.controlVar.GenerateLoad();
                            this.StoreInstruction(VirtualMachineInstruction.IINCR, index);
                            this.controlVar.GenerateStore();
                        }
                    }
                    else if (this.controlVar.DataType == TypeNode.DInt)
                    {
                        if (index == 0)
                            this.StoreInstruction(VirtualMachineInstruction.WINCR0);
                        else if (index == 1)
                            this.StoreInstruction(VirtualMachineInstruction.WINCR1);
                        else if (index == 2)
                            this.StoreInstruction(VirtualMachineInstruction.WINCR2);
                        else if (index == 3)
                            this.StoreInstruction(VirtualMachineInstruction.WINCR3);
                        else
                        {
                            this.controlVar.GenerateLoad();
                            this.StoreInstruction(VirtualMachineInstruction.IINCR, index);
                            this.controlVar.GenerateStore();
                        }
                    }
                    else
                    {
                        this.controlVar.GenerateLoad();
                        this.StoreInstruction(VirtualMachineInstruction.IINCR, index);
                        this.controlVar.GenerateStore();
                    }
                }
                else if (incr == -1)
                {
                    int index = this.controlVar.Location.Index;
                    if (this.controlVar.DataType == TypeNode.Int)
                    {
                        if (index == 0)
                            this.StoreInstruction(VirtualMachineInstruction.IDECR0);
                        else if (index == 1)
                            this.StoreInstruction(VirtualMachineInstruction.IDECR1);
                        else if (index == 2)
                            this.StoreInstruction(VirtualMachineInstruction.IDECR2);
                        else if (index == 3)
                            this.StoreInstruction(VirtualMachineInstruction.IDECR3);
                        else
                        {
                            this.controlVar.GenerateLoad();
                            this.StoreInstruction(VirtualMachineInstruction.IDECR, index);
                            this.controlVar.GenerateStore();
                        }
                    }
                    else if (this.controlVar.DataType == TypeNode.DInt)
                    {
                        if (index == 0)
                            this.StoreInstruction(VirtualMachineInstruction.WDECR0);
                        else if (index == 1)
                            this.StoreInstruction(VirtualMachineInstruction.WDECR1);
                        else if (index == 2)
                            this.StoreInstruction(VirtualMachineInstruction.WDECR2);
                        else if (index == 3)
                            this.StoreInstruction(VirtualMachineInstruction.WDECR3);
                        else
                        {
                            this.controlVar.GenerateLoad();
                            this.StoreInstruction(VirtualMachineInstruction.IDECR, index);
                            this.controlVar.GenerateStore();
                        }
                    }
                    else
                    {
                        this.controlVar.GenerateLoad();
                        this.StoreInstruction(VirtualMachineInstruction.IDECR, index);
                        this.controlVar.GenerateStore();
                    }
                }
                else
                {
                    this.increment.GenerateLoad();
                    controlVar.GenerateLoad();
                    this.StoreInstruction(VirtualMachineInstruction.IADD);
                    controlVar.GenerateStore();
                }
            }
        }

        private readonly MemoryObject controlVar;

        private readonly MemoryObject temporary;

        private readonly Expression initialValue;

        private readonly Expression stopCondition;

        private readonly Expression compoundStopValue;

        private readonly Expression increment;

        private readonly StatementList statementList;
    }

    public class ForLoopData
    {
        public MemoryObject ControlVariable { get; set; }

        public Expression InitialValue { get; set; }

        public Expression StopValue { get; set; }

        public Expression Increment { get; set; }
    }

    public enum ForLoopVariableType
    {
        NONE = 0x0,
        CONTROL_VARIABLE = 0x1,
        START_VARIABLE = 0x2,
        STOP_VARIABLE = 0x4,
        INCR_VARIABLE = 0x8
    }
}
