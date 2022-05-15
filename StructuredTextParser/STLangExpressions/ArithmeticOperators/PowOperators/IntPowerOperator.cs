using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class IntPowerOperator : BinaryOperator
    {
        public IntPowerOperator(Expression left, Expression right, TypeNode dataType)
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
                   this.StoreInstruction(VirtualMachineInstruction.ISQR);
               else if (exponent == 3)
               {
                   this.StoreInstruction(VirtualMachineInstruction.IDUPL);
                   this.StoreInstruction(VirtualMachineInstruction.ISQR);
                   this.StoreInstruction(VirtualMachineInstruction.IMUL);
               }
               else if (exponent == 4)
               {
                   this.StoreInstruction(VirtualMachineInstruction.ISQR);
                   this.StoreInstruction(VirtualMachineInstruction.ISQR);
               }
               else
               {
                   this.RightOperand.GenerateLoad();
                   this.StoreInstruction(StandardLibraryFunction.IEXPT);
               }
           }
           else
           {
               this.RightOperand.GenerateLoad();
               this.StoreInstruction(StandardLibraryFunction.IEXPT);
           }
        }

        public override int Priority
        {
            get { return 9; }
        }
    }
}
