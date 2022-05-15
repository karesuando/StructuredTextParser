using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class IntConstant : Constant<short>
    {
        public IntConstant(short value)
            : base(TypeNode.Int, value, value.ToString())
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public IntConstant(short value, string valueStr)
            : base(TypeNode.Int, value, valueStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public IntConstant(short value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            if (this.Value == -1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_N1);
            else if (this.Value == 0)
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

        public override bool ConstantForLoopBounds(List<ForLoopData> value)
        {
            return true;
        }

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return this.Value;
        }

        private readonly bool isSmallConstant;
    }
}
