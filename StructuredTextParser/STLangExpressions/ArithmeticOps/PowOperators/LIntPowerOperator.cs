using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LIntPowerOperator : BinaryOperator
    {
        public LIntPowerOperator(Expression left, Expression right, TypeNode dataType)
            : base(left, right, dataType, "**", VirtualMachineInstruction.CALL)
        {

        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
           this.LeftOperand.GenerateLoad();
           if (this.RightOperand.IsConstant)
           {
               int exponent = Convert.ToInt32(this.RightOperand.Evaluate());
               if (exponent == 2)
                   this.StoreInstruction(VirtualMachineInstruction.LSQR);
               else if (exponent == 3)
               {
                   this.StoreInstruction(VirtualMachineInstruction.LDUPL);
                   this.StoreInstruction(VirtualMachineInstruction.LSQR);
                   this.StoreInstruction(VirtualMachineInstruction.LMUL);
               }
               else if (exponent == 4)
               {
                   this.StoreInstruction(VirtualMachineInstruction.LSQR);
                   this.StoreInstruction(VirtualMachineInstruction.LSQR);
               }
               else
               {
                   this.RightOperand.GenerateLoad();
                   this.StoreInstruction(StandardLibraryFunction.LEXPT);
               }
           }
           else
           {
               this.RightOperand.GenerateLoad();
               this.StoreInstruction(StandardLibraryFunction.LEXPT);
           }
        }

        public override int Priority
        {
            get { return 9; }
        }
    }
}
