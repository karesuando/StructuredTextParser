using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;
using STLang.Statements;

namespace STLang.Expressions
{
    public class SIntConstant : Constant<sbyte>
    {
        public SIntConstant(sbyte value)
            : base(TypeNode.SInt, value, value.ToString())
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public SIntConstant(sbyte value, string valueStr)
            : base(TypeNode.SInt, value, valueStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public SIntConstant(sbyte value, TypeNode dataType, string valueStr)
            : base(dataType, value, valueStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
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

        public override int Evaluate(int bound, List<ForLoopData> value)
        {
            return this.Value;
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

        private readonly bool isSmallConstant;
    }
}
