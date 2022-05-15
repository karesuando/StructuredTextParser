using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.MemoryLayout;
using STLang.ConstantTokens;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RealConstant : Constant<float>
    {
        public RealConstant(float value)
            : base(TypeNode.Real, value, "REAL#" + value.ToString())
        {
            this.isSmallConstant = value == -1.0f || value == 0.0f || value == 1.0f;
        }

        public RealConstant(float value, string valueStr, TypeNode dataType)
            : base(dataType, value, valueStr)
        {
            this.isSmallConstant = value == -1.0f || value == 0.0f || value == 1.0f;
        }

        public RealConstant(float value, string stringValue)
            : base(TypeNode.Real, value, stringValue)
        {
            this.isSmallConstant = value == -1.0f || value == 0.0f || value == 1.0f;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.Value == -1.0f)
                this.StoreInstruction(VirtualMachineInstruction.FCONST_N1);
            else if (this.Value == 0.0f)
                this.StoreInstruction(VirtualMachineInstruction.FCONST_0);
            else if (this.Value == 1.0f)
                this.StoreInstruction(VirtualMachineInstruction.FCONST_1);
            else
            {
                if (this.Location == MemoryLocation.UndefinedLocation)
                    this.Location = this.GetConstantIndex();
                this.StoreInstruction(VirtualMachineInstruction.FCONST, this.Location.Index);
            }
        }

        public override bool IsZero
        {
            get { return this.Value == 0.0f; }
        }

        public override bool IsSmallConstant
        {
            get { return this.isSmallConstant; }
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        private readonly bool isSmallConstant;
    }
}
