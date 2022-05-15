using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class USIntConstant : Constant<byte>
    {
        public USIntConstant(byte value)
            : base(TypeNode.USInt, value, value.ToString())
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public USIntConstant(byte value, string valueStr)
            : base(TypeNode.USInt, value, valueStr)
        {
            this.isSmallConstant = value == 0 || value == 1;
        }

        public USIntConstant(byte value, TypeNode dataType, string valueStr)
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
