using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class ByteConstant : Constant<byte>
    {
        public ByteConstant(byte value)
            : base(TypeNode.Byte, value, value.ToString())
        {
          
        }

        public ByteConstant(byte value, string valueStr)
            : base(TypeNode.Byte, value, valueStr)
        {
            
        }

        public ByteConstant(byte value, TypeNode dataType, string valueStr)
            : base(TypeNode.Byte, value, valueStr)
        {

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
    }
}
