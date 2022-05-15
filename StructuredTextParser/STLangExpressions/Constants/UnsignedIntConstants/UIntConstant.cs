using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class UIntConstant : Constant<ushort>
    {
        public UIntConstant(ushort value)
            : base(TypeNode.UInt, value, value.ToString())
        {
            this.isSmallConstant =value == 0 || value == 1;
        }

        public UIntConstant(ushort value, string valueStr)
            : base(TypeNode.UInt, value, valueStr)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public UIntConstant(ushort value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            if (this.Value == 0)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            else if (this.Value == 1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            else
                this.StoreInstruction(VirtualMachineInstruction.ICONST, this.Value);
        }

        public override bool IsZero
        {
            get { return this.Value == 0; }
        }

        public override bool IsSmallConstant
        {
            get { return this.isSmallConstant; }
        }

        private readonly bool isSmallConstant;
    }
}
