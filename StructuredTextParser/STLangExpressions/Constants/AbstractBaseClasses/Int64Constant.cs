using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Statements;
using STLang.Extensions;
using STLang.MemoryLayout;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public abstract class Int64Constant : Constant<long>
    {

        public Int64Constant(TypeNode dataType, long value)
            : base(dataType, value)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        public Int64Constant(TypeNode dataType, long value, string exprStr)
            : base(dataType, value, exprStr)
        {
            this.isSmallConstant = value == -1 || value == 0 || value == 1;
        }

        static Int64Constant()
        {
            int64IndexCounter = 0;
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

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.Value == -1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_N1);
            else if (this.Value == 0)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            else if (this.Value == 1)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_1);
            else
            {
                if (this.Location == MemoryLocation.UndefinedLocation)
                    this.Location = this.GetConstantIndex();
                this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
            }
        }

        public static int int64IndexCounter; // UInt64Constant uses same counter

        public readonly bool isSmallConstant;
    }
}
