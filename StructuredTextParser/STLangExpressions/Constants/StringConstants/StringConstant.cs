using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.MemoryLayout;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class StringConstant : Constant<string>
    {
        public StringConstant(string value, TypeNode stringType)
            : base(stringType, value)
        {
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.Location == MemoryLocation.UndefinedLocation)
                this.Location = this.GetConstantIndex(this.Value);
            this.StoreInstruction(VirtualMachineInstruction.SCONST, this.Location.Index);
        }

        public override bool IsZero
        {
            get { return this.Value.Length == 0; }
        }

        public override string ToString()
        {
            return "'" + this.Value + "'";
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetAscIIBytes();
        }
    }
}
