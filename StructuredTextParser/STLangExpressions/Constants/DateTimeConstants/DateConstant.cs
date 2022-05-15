using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.ConstantTokens;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class DateConstant : Constant<DateTime>
    {
        public DateConstant(TokenDate date) 
            : base(TypeNode.Date, date.Value, date.ToString())
        {
        }

        public DateConstant(DateTime date)
            : base(TypeNode.Date, date, "D#" + date.ToString())
        {
        }

        public DateConstant(DateTime date, TypeNode dataType)
            : base(dataType, date, "D#" + date.ToString())
        {
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }
    }
}