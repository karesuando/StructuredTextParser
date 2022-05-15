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
    public class TimeConstant : Constant<TimeSpan>
    {
        public TimeConstant(TokenTime token) 
            : base(TypeNode.Time, token.Value, token.ToString())
        {
        }

        public TimeConstant(TimeSpan value)
            : base(TypeNode.Time, value, MakeString(value))
        {
        }

        public TimeConstant(TimeSpan value, TypeNode dataType)
            : base(dataType, value, MakeString(value))
        {
        }

        public TimeConstant(TimeSpan value, string stringValue)
            : base(TypeNode.Time, value, stringValue)
        {
        }

        public override byte[] GetBytes()
        {
            return this.Value.GetBytes();
        }

        public override bool IsZero
        {
            get { return this.Value == TimeSpan.Zero; }
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            if (this.IsZero)
                this.StoreInstruction(VirtualMachineInstruction.ICONST_0);
            else
                this.StoreInstruction(VirtualMachineInstruction.LCONST, this.Location.Index);
        }

        private static string MakeString(TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                return "T#0s";
            else { 
                string days = "";
                string hours = "";
                string minutes = "";
                string seconds = "";
                string millisec = "";
                if (time.Days > 0)
                    days = time.Days + "d";
                if (time.Hours > 0)
                    hours = time.Hours + "h";
                if (time.Minutes > 0)
                    minutes = time.Minutes + "m";
                if (time.Seconds > 0)
                    seconds = time.Seconds + "s";
                if (time.Milliseconds > 0)
                    millisec = time.Milliseconds + "ms";
                return "T#" + days + hours + minutes + seconds + millisec;
            }
        }
    }
}
