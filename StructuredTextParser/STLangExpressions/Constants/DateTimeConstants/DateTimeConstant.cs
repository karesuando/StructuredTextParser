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
    public class DateTimeConstant : Constant<DateTime>
    {
        public DateTimeConstant(TokenDateTime token) 
            : base(TypeNode.DateAndTime, token.Value, token.ToString())
        {
        }

        public DateTimeConstant(DateTime value)
            : base(TypeNode.DateAndTime, value, MakeString(value))
        {
        }

        public DateTimeConstant(DateTime value, TypeNode dataType)
            : base(dataType, value, MakeString(value))
        {
        }

        public DateTimeConstant(DateTime value, string stringValue)
            : base(TypeNode.DateAndTime, value, stringValue)
        {
        }

        private static string MakeString(DateTime dateTime)
        {
            string stringDT = dateTime.ToString();
            stringDT = stringDT.Replace(' ', '-');
            if (dateTime.Millisecond > 0)
                stringDT += "." + dateTime.Millisecond;
            return "DT#" + stringDT;
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
        }
    }
}
