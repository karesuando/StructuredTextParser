using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RealPowerOperator : BinaryOperator
    {
        public RealPowerOperator(Expression left, Expression right)
            : base(left, right, TypeNode.Real, "**", VirtualMachineInstruction.CALL)
        {

        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.LeftOperand.GenerateLoad();
            if (this.RightOperand.IsConstant)
            {
                if (this.RightOperand.DataType == TypeNode.Real)
                {
                    this.RightOperand.GenerateLoad();
                    this.StoreInstruction(StandardLibraryFunction.FEXPT);
                }
                else if (this.RightOperand.DataType.IsSignedIntType)
                {
                    int exponent = Convert.ToInt32(this.RightOperand.Evaluate());
                    if (exponent < 0)
                    {
                        exponent = -exponent;
                        this.StoreInstruction(VirtualMachineInstruction.FINV);
                    }
                    if (exponent == 2)
                        this.StoreInstruction(VirtualMachineInstruction.FSQR);
                    else if (exponent == 3)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.FDUPL);
                        this.StoreInstruction(VirtualMachineInstruction.FSQR);
                        this.StoreInstruction(VirtualMachineInstruction.FMUL);
                    }
                    else if (exponent == 4)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.FSQR);
                        this.StoreInstruction(VirtualMachineInstruction.FSQR);
                    }
                }
            }
            else if (this.RightOperand.DataType == TypeNode.Real)
            {
                this.RightOperand.GenerateLoad();
                this.StoreInstruction(StandardLibraryFunction.FEXPT);
            }
        }
    }
}
