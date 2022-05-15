using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class EnumConstant : Constant<ushort>
    {
        public EnumConstant(ushort value, TypeNode dataType, string strVal)
            : base(dataType, value, strVal)
        {
            this.value = value;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
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

        private readonly int value;
    }
}
