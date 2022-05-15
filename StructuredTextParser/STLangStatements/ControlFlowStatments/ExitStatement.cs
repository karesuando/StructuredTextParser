using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class ExitStatement : Statement
    {
        public override void GenerateCode(List<int> exitList)
        {
            int jumpLabel = ILC;
            this.StoreInstruction(VirtualMachineInstruction.JUMP, 0);
            exitList.Add(jumpLabel);
        }

        public override bool ContainsExit 
        {
            get { return true; } 
        }

        public override bool ControlFlowTerminates
        {
            get { return true; }
        }
    }
}
