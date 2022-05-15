using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.MemoryLayout;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class UInt32Constant : Constant<uint>
    {
        public UInt32Constant(TypeNode dataType, uint value)
            : base(dataType, value)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public UInt32Constant(TypeNode dataType, uint value, string exprStr)
            : base(dataType, value, exprStr)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public override bool IsZero
        {
            get { return this.Value == 0; }
        }

        public override bool IsSmallConstant
        {
            get { return this.isSmallConstant; }
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.Value == 0)
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

        private readonly bool isSmallConstant;
    }
}