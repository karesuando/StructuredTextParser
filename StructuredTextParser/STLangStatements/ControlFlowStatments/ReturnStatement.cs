using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.VMInstructions;

namespace STLang.Statements
{
    public class ReturnStatement : Statement
    {
        public ReturnStatement()
        {
        }

        public override bool FunctionReturns
        {
            get { return true; }
        }

        public override bool ControlFlowTerminates
        {
            get { return true; }
        }

        public override void GenerateCode(List<int> exitList)
        {
            this.StoreInstruction(VirtualMachineInstruction.RETN);
        }
    }
}
