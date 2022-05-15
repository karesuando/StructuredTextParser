using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class UInt64Constant : Constant<ulong>
    {

        public UInt64Constant(TypeNode dataType, ulong value)
            : base(dataType, value)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public UInt64Constant(TypeNode dataType, ulong value, string exprStr)
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
                this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
        }

        private readonly bool isSmallConstant;
    }
}