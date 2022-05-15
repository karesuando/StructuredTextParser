using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.VMInstructions;

namespace STLang.Expressions
{
    public class GenericTypeConvOperator : ConversionOperator
    {
        public GenericTypeConvOperator(Expression expression, TypeNode dataType, VirtualMachineInstruction opCode, int funcCode, string strExpr)
            : base(expression, strExpr, dataType, VirtualMachineInstruction.CONV, false)
        {
            this.functionCode = funcCode;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.Operand.GenerateLoad();
            this.StoreInstruction(this.OperationCode, this.functionCode);
        }

        private readonly int functionCode;
    }
}
