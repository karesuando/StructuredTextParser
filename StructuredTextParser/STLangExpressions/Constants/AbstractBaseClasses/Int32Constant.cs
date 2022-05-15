using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.MemoryLayout;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public abstract class Int32Constant : Constant<int>
    {
        public Int32Constant(TypeNode dataType, int value)
            : base(dataType, value)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public Int32Constant(TypeNode dataType, int value, string exprStr)
            : base(dataType, value, exprStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        static Int32Constant()
        {
            int32IndexCounter = 0;
        }

        public override bool IsZero
        {
            get { return this.Value == 0; }
        }

        public override bool IsSmallConstant
        {
            get { return this.isSmallConstant; }
        }

        public override bool ConstantForLoopBounds(List<ForLoopData> value)
        {
            return true;
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return this.Value;
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        public override void GenerateLoad(List<int> trueBranchl, List<int> falseBranch)
        {
            if (this.Value == -1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_N1);
            else if (this.Value == 0)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            else if (this.Value == 1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            else
            {
                if (this.Location == MemoryLocation.UndefinedLocation)
                    this.Location = this.GetConstantIndex();
                this.StoreInstruction(VirtualMachineInstruction.WCONST, this.Location.Index);
            }
        }

        public static int int32IndexCounter;

        private readonly bool isSmallConstant;
    }
}
