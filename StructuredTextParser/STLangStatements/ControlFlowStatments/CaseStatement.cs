using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.Expressions;
using System.Runtime.InteropServices;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class CaseStatement : Statement
    {
        public CaseStatement(Expression controlExpr, List<CaseElement> caseElemList)
        {
            this.controlExpr = controlExpr;
            this.caseElemList = caseElemList;
            this.defaultStatement = null;
            if (!this.controlExpr.IsConstant)
            {
                // Check if constants span the entire domain of definition of the 
                // control expression datatype
                StatementList statementList;
                this.IsFunctionValueDefined = false;
                foreach (CaseElement caseElement in this.caseElemList)
                {
                    statementList = caseElement.StatementList;
                    if (!this.ContainsExit)
                        this.ContainsExit = statementList.ContainsExit;
                }
            }
            else
            {
                NumericLabel caseLabel = new NumericLabel(this.controlExpr);
                foreach (CaseElement caseElement in this.caseElemList)
                {
                    if (caseElement.Contains(caseLabel))
                    {
                        StatementList statementList = caseElement.StatementList;
                        this.IsFunctionValueDefined = statementList.IsFunctionValueDefined;
                        this.FunctionReturns = statementList.POUReturns;
                        this.ContainsExit = statementList.ContainsExit;
                        this.ControlFlowTerminates = statementList.ControlFlowTerminates;
                        break;
                    }
                }
            }
        }

        public CaseStatement(Expression controlExpr, List<CaseElement> caseElemList, CaseElement defaultStat)
        {
            this.controlExpr = controlExpr;
            this.defaultStatement = defaultStat.StatementList;
            this.caseElemList = caseElemList;
            if (this.controlExpr.IsConstant)
             {
                NumericLabel caseLabel = new NumericLabel(this.controlExpr);
                foreach (CaseElement caseElement in this.caseElemList)
                {
                    if (caseElement.Contains(caseLabel))
                    {
                        StatementList statementList = caseElement.StatementList;
                        this.IsFunctionValueDefined = statementList.IsFunctionValueDefined;
                        this.ContainsExit = statementList.ContainsExit;
                        this.FunctionReturns = statementList.POUReturns;
                        this.ControlFlowTerminates = statementList.ControlFlowTerminates;
                        return;
                    }
                }
                this.IsFunctionValueDefined = this.defaultStatement.IsFunctionValueDefined;
                this.ContainsExit = this.defaultStatement.ContainsExit;
                this.FunctionReturns = this.defaultStatement.POUReturns;
                this.ControlFlowTerminates = this.defaultStatement.ControlFlowTerminates;
            }
            else if (caseElemList.Count == 0)
            {
                this.FunctionReturns = this.defaultStatement.POUReturns;
                this.IsFunctionValueDefined = this.defaultStatement.IsFunctionValueDefined;
                this.ContainsExit = this.defaultStatement.ContainsExit;
                this.ControlFlowTerminates = this.defaultStatement.ControlFlowTerminates;
            }
            else
            {
                StatementList statList;
                this.FunctionReturns = true;
                this.ContainsExit = false;
                this.IsFunctionValueDefined = true;
                this.ControlFlowTerminates = true;
                foreach (CaseElement caseElement in caseElemList)
                {
                    statList = caseElement.StatementList;
                    if (!this.ContainsExit)
                        this.ContainsExit = statList.ContainsExit;
                    if (this.IsFunctionValueDefined)
                        this.IsFunctionValueDefined = statList.IsFunctionValueDefined;
                    if (this.FunctionReturns)
                        this.FunctionReturns = statList.POUReturns;
                    if (this.ControlFlowTerminates)
                        this.ControlFlowTerminates = statList.ControlFlowTerminates;
                }
                if (this.FunctionReturns)
                    this.FunctionReturns = this.defaultStatement.POUReturns;
                if (this.IsFunctionValueDefined)
                    this.IsFunctionValueDefined = this.defaultStatement.IsFunctionValueDefined;
                if (!this.ContainsExit)
                    this.ContainsExit = this.defaultStatement.ContainsExit;
                if (this.ControlFlowTerminates)
                    this.ControlFlowTerminates = this.defaultStatement.ControlFlowTerminates;
            }
        }

        private readonly Expression controlExpr;

        private readonly List<CaseElement> caseElemList;

        private readonly StatementList defaultStatement;

        private const double MAX_SPARSENESS_QUOT = 3.0d;

        private const int MAX_JUMPTABLE_SIZE = (1 << 24) - 1;

        private void StoreJumpTable(ushort[] jumpTable, int offset)
        {
            List<byte> byteList = new List<byte>();

            foreach (ushort target in jumpTable)
            {
                byteList.AddRange(BitConverter.GetBytes(target));
            }
            StoreByteArrayAtOffset(byteList.ToArray(), offset);
        }

        private void StoreSearchTable<T>(T[] searchTable, int sizeOfT, int offset)
        {
            byte[] bytes = ToByteArray(searchTable);
            StoreByteArrayAtOffset(bytes, offset);
        }

        private void StoreSwitchHeader(SwitchHeader header, int offset)
        {
            byte[] bytes = ToByteArray(header);
            StoreByteArrayAtOffset(bytes, offset);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SearchTableEntryI1
        {
            [MarshalAs(UnmanagedType.I1)]
            public sbyte Value;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SearchTableEntryI2
        {
            [MarshalAs(UnmanagedType.I2)]
            public short Value;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SearchTableEntryI4
        {
            [MarshalAs(UnmanagedType.I4)]
            public int Value;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SearchTableEntryI8
        {
            [MarshalAs(UnmanagedType.I8)]
            public long Value;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SwitchHeader
        {
            [MarshalAs(UnmanagedType.I8)]
            public long minValue;
            [MarshalAs(UnmanagedType.I8)]
            public long maxValue;
            [MarshalAs(UnmanagedType.I4)]
            public int IsInt64;
            [MarshalAs(UnmanagedType.I4)]
            public int defaultOffset;
            [MarshalAs(UnmanagedType.I4)]
            public int elementSize;
            [MarshalAs(UnmanagedType.I4)]
            public int tableSize;
        }

        private SearchTableEntryI1[] CreateSearchTable(List<sbyte>[] caseLabels, ulong count)
        {
            int i = 0;
            ushort listIndex = 0;
            SearchTableEntryI1[] searchTable = new SearchTableEntryI1[count];
            foreach (List<sbyte> labelList in caseLabels)
            {
                foreach (sbyte label in labelList)
                {
                    searchTable[i].Value = label;
                    searchTable[i].Offset = listIndex;
                    i++;
                }
                listIndex++;
            }
            searchTable = searchTable.OrderBy(label => label.Value).ToArray();
            return searchTable;
        }

        private SearchTableEntryI2[] CreateSearchTable(List<short>[] caseLabels, ulong count)
        {
            int i = 0;
            ushort listIndex = 0;
            SearchTableEntryI2[] searchTable = new SearchTableEntryI2[count];
            foreach (List<short> labelList in caseLabels)
            {
                foreach (short label in labelList)
                {
                    searchTable[i].Value = label;
                    searchTable[i].Offset = listIndex;
                    i++;
                }
                listIndex++;
            }
            searchTable = searchTable.OrderBy(label => label.Value).ToArray();
            return searchTable;
        }

        private SearchTableEntryI2[] CreateSearchTable(List<ushort>[] caseLabels, ulong count)
        {
            int i = 0;
            ushort listIndex = 0;
            SearchTableEntryI2[] searchTable = new SearchTableEntryI2[count];
            foreach (List<ushort> labelList in caseLabels)
            {
                foreach (ushort label in labelList)
                {
                    searchTable[i].Value = (short)label;
                    searchTable[i].Offset = listIndex;
                    i++;
                }
                listIndex++;
            }
            searchTable = searchTable.OrderBy(label => label.Value).ToArray();
            return searchTable;
        }

        private SearchTableEntryI4[] CreateSearchTable(List<int>[] caseLabels, ulong count)
        {
            int i = 0;
            ushort listIndex = 0;
            SearchTableEntryI4[] searchTable = new SearchTableEntryI4[count];
            foreach (List<int> labelList in caseLabels)
            {
                foreach (int label in labelList)
                {
                    searchTable[i].Value = label;
                    searchTable[i].Offset = listIndex;
                    i++;
                }
                listIndex++;
            }
            searchTable = searchTable.OrderBy(label => label.Value).ToArray();
            return searchTable;
        }

        private SearchTableEntryI8[] CreateSearchTable(List<long>[] caseLabels, ulong count)
        {
            int i = 0;
            ushort listIndex = 0;
            SearchTableEntryI8[] searchTable = new SearchTableEntryI8[count];
            foreach (List<long> labelList in caseLabels)
            {
                foreach (int label in labelList)
                {
                    searchTable[i].Value = label;
                    searchTable[i].Offset = listIndex;
                    i++;
                }
                listIndex++;
            }
            searchTable = searchTable.OrderBy(label => label.Value).ToArray();
            return searchTable;
        }

        private SwitchHeader CreateSwitchHeader(long min, long max, uint elemSize, int tableSize, int isInt64)
        {
            SwitchHeader header = new SwitchHeader();
            header.minValue = min;
            header.maxValue = max;
            header.IsInt64 = isInt64;
            header.elementSize = (ushort)elemSize;
            header.tableSize = tableSize;
            return header;
        }

        private void FillInJumpTarget(ushort[] jumpTable, List<sbyte> caseLabelList, ushort offset, sbyte minValue)
        {
            foreach (sbyte caseLabel in caseLabelList)
            {
                jumpTable[caseLabel - minValue] = offset;
            }
        }

        private void FillInJumpTarget(ushort[] jumpTable, List<short> caseLabelList, ushort offset, short minValue)
        {
            foreach (short caseLabel in caseLabelList)
            {
                jumpTable[caseLabel - minValue] = offset;
            }
        }

        private void FillInJumpTarget(ushort[] jumpTable, List<ushort> caseLabelList, ushort offset, ushort minValue)
        {
            foreach (ushort caseLabel in caseLabelList)
            {
                jumpTable[caseLabel - minValue] = offset;
            }
        }

        private void FillInJumpTarget(ushort[] jumpTable, List<int> caseLabelList, ushort offset, int minValue)
        {
            foreach (int caseLabel in caseLabelList)
            {
                jumpTable[caseLabel - minValue] = offset;
            }
        }

        private void FillInJumpTarget(ushort[] jumpTable, List<long> caseLabelList, ushort offset, long minValue)
        {
            foreach (long caseLabel in caseLabelList)
            {
                jumpTable[caseLabel - minValue] = offset;
            }
        }

        private void FillInDefaultJumpTarget(ushort[] jumpTable,  ushort offset)
        {
            int tableSize = jumpTable.Count();
            for (int j = 0; j < tableSize; j++)
            {
                if (jumpTable[j] == 0)
                    jumpTable[j] = offset;
            }
        }

        public override void GenerateCode(List<int> exitList)
        {
            if (this.caseElemList.Count == 0)
            {
                if (this.defaultStatement != null)
                    this.defaultStatement.GenerateCode(exitList);
                return;
            }
            ulong count;
            int switchOffset = 0;
            double sparsenessQuot;
            bool controlFlowTerminates;
            SwitchHeader switchHeader = new SwitchHeader();
            TypeNode ctrlExprDataType = this.controlExpr.DataType;
           
            int caseElemCount = this.caseElemList.Count;
            List<int> jumpEndCase = new List<int>();
            this.controlExpr.GenerateLoad();
            int switchLabel = ILC;
            if (ctrlExprDataType == TypeNode.SInt)
            {
                sbyte min, max;
                List<sbyte>[] caseLabels;
                caseLabels = this.CreateCaseLabelLists<sbyte>(this.caseElemList);
                this.GetMinMax(caseLabels, out min, out max, out count);
                int valueSpan = max - min + 1;
                sparsenessQuot = valueSpan / (double)count;
                if (sparsenessQuot < MAX_SPARSENESS_QUOT && valueSpan <= MAX_JUMPTABLE_SIZE)
                {
                    int i = 0;
                    ushort statementOffset;
                    ushort[] jumpTable = new ushort[valueSpan];
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.SInt.Size, valueSpan, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int tableSize = valueSpan * sizeof(ushort);
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.L_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset = (ushort)(ILC - switchLabel);
                        this.FillInJumpTarget(jumpTable, caseLabels[i++], statementOffset, min);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    statementOffset = (ushort)(ILC - switchLabel);
                    switchHeader.defaultOffset = statementOffset;
                    this.FillInDefaultJumpTarget(jumpTable, statementOffset);
                    this.StoreJumpTable(jumpTable, tableOffset);
                }
                else
                {
                    int i = 0;
                    ushort[] statementOffset = new ushort[caseElemCount];
                    SearchTableEntryI1[] searchTable;
                    searchTable = this.CreateSearchTable(caseLabels, count);
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.SInt.Size, (int)count, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int sizeOfEntry = Marshal.SizeOf(searchTable[0]);
                    int tableSize = (int)count * sizeOfEntry;
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.T_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset[i++] = (ushort)(ILC - switchLabel);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    for (ulong j = 0; j < count; j++ )
                    {
                        int caseElemIndex = searchTable[j].Offset;
                        searchTable[j].Offset = statementOffset[caseElemIndex];
                    }
                    this.StoreSearchTable(searchTable, sizeOfEntry, tableOffset);
                }
            }
            else if (ctrlExprDataType == TypeNode.Int)
            {
                short min, max;
                List<short>[] caseLabels = this.CreateCaseLabelLists<short>(this.caseElemList);
                this.GetMinMax(caseLabels, out min, out max, out count);
                sparsenessQuot = (max - min + 1) / (double)count;
                int valueSpan = max - min + 1;
                sparsenessQuot = valueSpan / (double)count;
                if (sparsenessQuot < MAX_SPARSENESS_QUOT && valueSpan <= MAX_JUMPTABLE_SIZE)
                {
                    int i = 0;
                    ushort statementOffset;
                    ushort[] jumpTable = new ushort[valueSpan];
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.Int.Size, (int)valueSpan, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int tableSize = valueSpan * sizeof(ushort);
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.L_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset = (ushort)(ILC - switchLabel);
                        this.FillInJumpTarget(jumpTable, caseLabels[i++], statementOffset, min);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    statementOffset = (ushort)(ILC - switchLabel);
                    switchHeader.defaultOffset = statementOffset;
                    this.FillInDefaultJumpTarget(jumpTable, statementOffset);
                    this.StoreJumpTable(jumpTable, tableOffset);
                }
                else
                {
                    int i = 0;
                    ushort[] statementOffset = new ushort[caseElemCount];
                    SearchTableEntryI2[] searchTable;
                    searchTable = this.CreateSearchTable(caseLabels, count);
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.Int.Size, (int)count, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int sizeOfEntry = Marshal.SizeOf(searchTable[0]);
                    int tableSize = (int)count * sizeOfEntry;
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(short));
                    this.StoreInstruction(VirtualMachineInstruction.T_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset[i++] = (ushort)(ILC - switchLabel);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    for (ulong j = 0; j < count; j++)
                    {
                        int caseElemIndex = searchTable[j].Offset;
                        searchTable[j].Offset = statementOffset[caseElemIndex];
                    }
                    this.StoreSearchTable(searchTable, sizeOfEntry, tableOffset);
                }
            }
            else if (ctrlExprDataType == TypeNode.DInt)
            {
                int i = 0;
                int min, max;
                List<int>[] caseLabels = this.CreateCaseLabelLists<int>(this.caseElemList);
                this.GetMinMax(caseLabels, out min, out max, out count);
                sparsenessQuot = (max - min + 1) / (double)count;
                int valueSpan = max - min + 1;
                sparsenessQuot = valueSpan / (double)count;
                if (sparsenessQuot < MAX_SPARSENESS_QUOT && valueSpan <= MAX_JUMPTABLE_SIZE)
                {
                    ushort statementOffset;
                    ushort[] jumpTable = new ushort[valueSpan];
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.DInt.Size, (int)valueSpan, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int tableSize = valueSpan * sizeof(ushort);
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.L_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset = (ushort)(ILC - switchLabel);
                        this.FillInJumpTarget(jumpTable, caseLabels[i++], statementOffset, min);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    statementOffset = (ushort)(ILC - switchLabel);
                    switchHeader.defaultOffset = statementOffset;
                    this.FillInDefaultJumpTarget(jumpTable, statementOffset);
                    this.StoreJumpTable(jumpTable, tableOffset);
                }
                else
                {
                    ushort[] statementOffset = new ushort[caseElemCount];
                    SearchTableEntryI4[] searchTable;
                    searchTable = this.CreateSearchTable(caseLabels, count);
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.DInt.Size, (int)count, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int sizeOfEntry = Marshal.SizeOf(searchTable[0]);
                    int tableSize = (int)count * sizeOfEntry;
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(int));
                    this.StoreInstruction(VirtualMachineInstruction.T_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset[i++] = (ushort)(ILC - switchLabel);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    for (ulong j = 0; j < count; j++)
                    {
                        int caseElemIndex = searchTable[j].Offset;
                        searchTable[j].Offset = statementOffset[caseElemIndex];
                    }
                    this.StoreSearchTable(searchTable, sizeOfEntry, tableOffset);
                }
            }
            else if (ctrlExprDataType == TypeNode.LInt)
            {
                long min, max;
                List<long>[] caseLabels = this.CreateCaseLabelLists<long>(this.caseElemList);
                this.GetMinMax(caseLabels, out min, out max, out count);
                long valueSpan = max - min + 1;
                sparsenessQuot = valueSpan / (double)count;
                if (sparsenessQuot < MAX_SPARSENESS_QUOT && valueSpan <= MAX_JUMPTABLE_SIZE)
                {
                    int i = 0;
                    ushort statementOffset;
                    ushort[] jumpTable = new ushort[valueSpan];
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.LInt.Size, (int)valueSpan, 1);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int tableSize = (int)valueSpan * sizeof(ushort);
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.L_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset = (ushort)(ILC - switchLabel);
                        this.FillInJumpTarget(jumpTable, caseLabels[i++], statementOffset, min);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    statementOffset = (ushort)(ILC - switchLabel);
                    switchHeader.defaultOffset = statementOffset;
                    this.FillInDefaultJumpTarget(jumpTable, statementOffset);
                    this.StoreJumpTable(jumpTable, tableOffset);
                }
                else
                {
                    int i = 0;
                    ushort[] statementOffset = new ushort[caseElemCount];
                    SearchTableEntryI8[] searchTable;
                    searchTable = this.CreateSearchTable(caseLabels, count);
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.LInt.Size, (int)count, 1);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int sizeOfEntry = Marshal.SizeOf(searchTable[0]);
                    int tableSize = (int)count * sizeOfEntry;
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(long));
                    this.StoreInstruction(VirtualMachineInstruction.T_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset[i++] = (ushort)(ILC - switchLabel);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    for (ulong j = 0; j < count; j++)
                    {
                        int caseElemIndex = searchTable[j].Offset;
                        searchTable[j].Offset = statementOffset[caseElemIndex];
                    }
                    this.StoreSearchTable(searchTable, sizeOfEntry, tableOffset);
                }
            }
            else if (ctrlExprDataType.IsEnumeratedType)
            {
                int i = 0;
                ushort min, max;
                List<ushort>[] caseLabels = this.CreateCaseLabelLists<ushort>(this.caseElemList);
                this.GetMinMax(caseLabels, out min, out max, out count);
                int valueSpan = max - min + 1;
                sparsenessQuot = valueSpan / (double)count;
                if (sparsenessQuot < MAX_SPARSENESS_QUOT && valueSpan <= MAX_JUMPTABLE_SIZE)
                {
                    ushort statementOffset;
                    ushort[] jumpTable = new ushort[valueSpan];
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.UInt.Size, valueSpan, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int tableSize = valueSpan * sizeof(ushort);
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.L_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset = (ushort)(ILC - switchLabel);
                        this.FillInJumpTarget(jumpTable, caseLabels[i++], statementOffset, min);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    statementOffset = (ushort)(ILC - switchLabel);
                    switchHeader.defaultOffset = statementOffset;
                    this.FillInDefaultJumpTarget(jumpTable, statementOffset);
                    this.StoreJumpTable(jumpTable, tableOffset);
                }
                else
                {
                    ushort[] statementOffset = new ushort[caseElemCount];
                    SearchTableEntryI2[] searchTable;
                    searchTable = this.CreateSearchTable(caseLabels, count);
                    switchHeader = this.CreateSwitchHeader(min, max, TypeNode.UInt.Size, (int)count, 0);
                    switchOffset = this.AllocateMemory(Marshal.SizeOf(switchHeader), sizeof(long));
                    int sizeOfEntry = Marshal.SizeOf(searchTable[0]);
                    int tableSize = (int)count * sizeOfEntry;
                    int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                    this.StoreInstruction(VirtualMachineInstruction.T_SWITCH, switchOffset);
                    foreach (CaseElement caseElement in this.caseElemList)
                    {
                        statementOffset[i++] = (ushort)(ILC - switchLabel);
                        caseElement.GenerateStatements(exitList);
                        controlFlowTerminates = caseElement.StatementList.ControlFlowTerminates;
                        if ((i <= caseElemCount - 1 || defaultStatement != null) && !controlFlowTerminates)
                        {
                            jumpEndCase.Add(ILC);
                            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
                        }
                    }
                    for (ulong j = 0; j < count; j++)
                    {
                        int caseElemIndex = searchTable[j].Offset;
                        searchTable[j].Offset = statementOffset[caseElemIndex];
                    }
                    this.StoreSearchTable(searchTable, sizeOfEntry, tableOffset);
                }
            }
            if (this.defaultStatement != null)
                this.defaultStatement.GenerateCode();
            this.BackPatch(jumpEndCase, (uint)ILC);
            this.StoreSwitchHeader(switchHeader, switchOffset);
        }

        private void GetMinMax<T>(List<T>[] caseLabels, out T min, out T max, out ulong count) where T : struct, IComparable<T>
        {
            count = 0;
            min = max = caseLabels[0][0];
            foreach (List<T> constList in caseLabels)
            {
                foreach (T constant in constList)
                {
                    if (constant.CompareTo(min) < 0)
                        min = constant;
                    else if (constant.CompareTo(max) > 0)
                        max = constant;
                }
                count += (ulong)constList.Count;
            }
        }

        private List<T>[] CreateCaseLabelLists<T>(List<CaseElement> caseElemList) where T : struct, IComparable<T>
        {
            int i = 0;
            List<T> caseLabelList;
            List<T>[] caseLabels = new List<T>[caseElemList.Count];
            StatementList[] statementList = new StatementList[caseElemList.Count];
            foreach (CaseElement caseElem in caseElemList)
            {
                statementList[i] = caseElem.StatementList;
                caseLabelList = new List<T>();
                foreach (CaseLabel caseLabel in caseElem.LabelList)
                {
                    if (caseLabel is SubrangeLabel)
                    {
                        SubrangeLabel subRange = (SubrangeLabel)caseLabel;
                        T[] interval = (T[])subRange.CreateInterval();
                        caseLabelList.AddRange(interval);
                    }
                    else
                    {
                        NumericLabel simpleLabel = (NumericLabel)caseLabel;
                        T value = (T)simpleLabel.Value.Evaluate();
                        caseLabelList.Add(value);
                    }
                }
                caseLabels[i++] = caseLabelList;
            }
            return caseLabels;
        }
    }

    public class CaseElement
    {
        public List<CaseLabel> LabelList { get; set; }

        public StatementList StatementList { get; set; }

        public bool Contains(NumericLabel number)
        {
            return this.LabelList.Find(caseLabel => caseLabel.AreDisjoint(number)) != null;
        }

        public void GenerateStatements(List<int> exitList)
        {
            this.StatementList.GenerateCode(exitList);
        }
    }
}
