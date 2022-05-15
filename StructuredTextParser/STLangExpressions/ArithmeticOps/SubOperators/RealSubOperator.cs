using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class RealSubOperator : BinaryOperator
    {
        public RealSubOperator(Expression arg1, Expression arg2)
            : base(arg1, arg2, TypeNode.Real, " - ", VirtualMachineInstruction.FSUB)
        {
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            throw new NotImplementedException();
        }

        public override int Priority
        {
            get { return 6; }
        }
    }
}
