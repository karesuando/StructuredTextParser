using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using STLang.VMInstructions;
using System.Runtime.InteropServices;

namespace STLang.Expressions
{
    public class MUXFunctionCall : StandardFunctionCall
    {
        public MUXFunctionCall(Expression[] inputs, TypeNode resultDataType, string exprStr)
            : base(inputs, resultDataType, StandardLibraryFunction.NONE, exprStr)
        {
            this.inputCount = inputs.Length - 1;
        }

        private void StoreJumpTable(ushort[] jumpTable, int offset)
        {
            List<byte> byteList = new List<byte>();

            foreach (ushort target in jumpTable)
            {
                byteList.AddRange(BitConverter.GetBytes(target));
            }
            StoreByteArrayAtOffset(byteList.ToArray(), offset);
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            Expression selector = this.arguments[0];

            if (selector.IsConstant)
            {
                int k = Convert.ToInt32(selector.Evaluate());
                if (k >= 0 && k < this.inputCount)
                    this.arguments[k + 1].GenerateLoad();
                else {
                    string msg = "MUX: Constant selector value out of range.";
                    throw new STLangCompilerError(msg);
                }
            }
            else if (this.inputCount < MIN_JUMP_TABLE_LENGTH)
            {
                selector.GenerateLoad();
                this.StoreInstruction(VirtualMachineInstruction.IDUPL);
                int testLabel0 = ILC;
                this.StoreInstruction(VirtualMachineInstruction.IJEQZ);
                this.arguments[1].GenerateLoad();
                int jumpLabel = ILC;
                this.StoreInstruction(VirtualMachineInstruction.JUMP);
                this.BackPatch(testLabel0, ILC);
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
                int testLabel1 = ILC;
                this.StoreInstruction(VirtualMachineInstruction.IJEQ, testLabel0 + 1);  // If k > 1 load input0
                this.arguments[2].GenerateLoad();
                this.BackPatch(jumpLabel, ILC);
            }
            else {
                // MUX(k, Input0, Input1, ..., InputN-1)
                //
                // Generate jump table for k = 0, 1, 2, ..., n - 1 inputs
                //
                int inputPos = 0;
                List<int> jumpLabels = new List<int>();
                ushort[] jumpTable = new ushort[this.inputCount];
                int tableSize = this.inputCount * sizeof(ushort);
                int tableOffset = this.AllocateMemory(tableSize, sizeof(ushort));
                selector.GenerateLoad();
                if (selector.DataType.Size == TypeNode.LInt.Size)
                    this.StoreInstruction(VirtualMachineInstruction.L2I);
                this.StoreInstruction(VirtualMachineInstruction.ICONST, this.inputCount);
                int switchLabel = ILC;
                this.StoreInstruction(VirtualMachineInstruction.M_SWITCH, tableOffset);
                foreach (Expression input in this.arguments.Skip(1))
                {
                    jumpTable[inputPos] = (ushort)(ILC - switchLabel);
                    input.GenerateLoad();
                    if (inputPos < this.inputCount - 1)
                    {
                        jumpLabels.Add(ILC);
                        this.StoreInstruction(VirtualMachineInstruction.JUMP);
                    }
                    inputPos++;
                }
                uint endMUXLabel = (uint)ILC;
                this.BackPatch(jumpLabels, endMUXLabel);
                this.StoreJumpTable(jumpTable, tableOffset);
            }
        }

        private readonly int inputCount;

        private const int MIN_JUMP_TABLE_LENGTH = 3;
    }
}
