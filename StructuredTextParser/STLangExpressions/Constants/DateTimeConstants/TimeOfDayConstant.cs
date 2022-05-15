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
    public class TimeOfDayConstant : Constant<TimeSpan>
    {
        public TimeOfDayConstant(TokenTOD token) 
            : base(TypeNode.TimeOfDay, token.Value, token.ToString())
        {
        }

        public TimeOfDayConstant(TimeSpan value)
            : base(TypeNode.TimeOfDay, value, MakeString(value))
        {
        }

        public TimeOfDayConstant(TimeSpan value, TypeNode dataType)
            : base(dataType, value, MakeString(value))
        {
        }

        public TimeOfDayConstant(TimeSpan value, string stringValue)
            : base(TypeNode.TimeOfDay, value, stringValue)
        {
        }

        private static string MakeString(TimeSpan time)
        {
            string hours = time.Hours.ToString("00");
            string minutes = time.Minutes.ToString("00");
            string seconds = time.Minutes.ToString("00");
            string todStr = string.Format("TOD#{0}:{1}:{2}", hours, minutes, seconds);
            if (time.Milliseconds > 0)
            {
                string millsec = time.Milliseconds.ToString("000");
                todStr += "." + millsec;
            }
            return todStr;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.IsZero)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            else
                this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
        }

        public override bool IsZero
        {
            get { return this.Value == TimeSpan.Zero; }
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }
    }
}