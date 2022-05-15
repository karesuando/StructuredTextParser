using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class LRealPowerOperator : BinaryOperator
    {
        public LRealPowerOperator(Expression left, Expression right)
            : base(left, right, TypeNode.LReal, "**", VirtualMachineInstruction.CALL)
        {

        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.LeftOperand.GenerateLoad();
            if (this.RightOperand.IsConstant)
            {
                if (this.RightOperand.DataType == TypeNode.LReal)
                {
                    this.RightOperand.GenerateLoad();
                    this.StoreInstruction(StandardLibraryFunction.DEXPT);
                }
                else if (this.RightOperand.DataType.IsSignedIntType)
                {
                    int exponent = Convert.ToInt32(this.RightOperand.Evaluate());
                    if (exponent < 0)
                    {
                        exponent = -exponent;
                        this.StoreInstruction(VirtualMachineInstruction.DINV);
                    }
                    if (exponent == 2)
                        this.StoreInstruction(VirtualMachineInstruction.DSQR);
                    else if (exponent == 3)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.DDUPL);
                        this.StoreInstruction(VirtualMachineInstruction.DSQR);
                        this.StoreInstruction(VirtualMachineInstruction.DMUL);
                    }
                    else if (exponent == 4)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.DSQR);
                        this.StoreInstruction(VirtualMachineInstruction.DSQR);
                    }
                }
            }
            else if (this.RightOperand.DataType.IsSignedIntType)
            {
                this.RightOperand.GenerateLoad();
                this.StoreInstruction(StandardLibraryFunction.DEXPT);
            }
            else if (this.RightOperand.DataType == TypeNode.LReal)
            {
                this.RightOperand.GenerateLoad();
                this.StoreInstruction(StandardLibraryFunction.DEXPT);
            }
        }

        public override int Priority
        {
            get { return 9; }
        }
    }
}
