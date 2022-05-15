using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.MemoryLayout;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class WordConstant : Constant<ushort>
    {
        public WordConstant(ushort value)
            : base(TypeNode.Word, value, value.ToString())
        {
        }

        public WordConstant(ushort value, string valueStr)
            : base(TypeNode.Word, value, valueStr)
        {
        }

        public WordConstant(ushort value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
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
                this.StoreInstruction(VirtualMachineInstruction.ICONST, this.Value);
            }
        }
    }
}

