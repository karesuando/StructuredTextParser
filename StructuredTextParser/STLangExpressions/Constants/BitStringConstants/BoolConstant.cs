using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class BoolConstant : Constant<bool>
    {
        public BoolConstant(bool value)
            : base(TypeNode.Bool, value, value.ToString())
        {
            this.value = value;
        }

        public BoolConstant(bool value, string valueStr)
            : base(TypeNode.Bool, value, valueStr)
        {
            this.value = value;
        }

        public BoolConstant(bool value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
            this.value = value;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (value)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            else
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
        }

        private readonly bool value;
    }
}
