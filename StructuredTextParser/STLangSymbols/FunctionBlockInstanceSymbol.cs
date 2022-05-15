using System;
using System.Linq;
using System.Collections.Generic;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.MemoryLayout;
using QUT.Gppg;
using STLang.ParserUtility;

namespace STLang.Symbols
{
    public class FunctionBlockInstanceSymbol : CompoundInstanceSymbol
    {
        public FunctionBlockInstanceSymbol(string name, TypeNode dataType, STVarType varType,
                            STVarQualifier varQual, STDeclQualifier edgeQual, Expression initValue,
                            Dictionary<string, InstanceSymbol> members, InstanceSymbol firstMem, 
                            int position, FramePointer fp)
            : base(name, dataType, varType, varQual, edgeQual, initValue, members, firstMem, position)
        {
            this.framePointer = fp;
        }

        public override Expression MakeSyntaxTreeNode(LexLocation loc)
        {
            return new MemoryObject(this.Name, this.Location, this.DataType, this);
        }

        private void SortInputArguments(List<Expression> arguments, out List<Expression> inputs, out List<Expression> outputs)
        {
            List<POUParameter> inputs2;
            inputs2 = new List<POUParameter>();
            outputs = new List<Expression>();
            foreach (Expression argument in arguments)
            {
                if (argument is InputParameter)
                    inputs2.Add(argument as POUParameter);
                else if (argument is OutputParameter)
                    outputs.Add(argument);
            }
            inputs = (from input in inputs2
                      orderby input.Position
                      select input.RValue).ToList();
        }

        public override Expression MakeSyntaxTreeNode(List<Expression> arguments, LexLocation loc)
        {
            if (arguments.Count == 0)
            {
                InstanceSymbol member = this.firstMember;
                List<Expression> inputs = new List<Expression>();
                while (member != null)
                {
                    arguments.Add(member.DataType.DefaultValue);
                    if (member.VariableType == STVarType.VAR_INOUT)
                        Report.SemanticError(78, this.Name, member.Name, loc);
                    member = member.Next;
                }
            }
            else
            {
                List<Expression> inputs, outputs;
                
                this.SortInputArguments(arguments, out inputs, out outputs);
                arguments = inputs.Concat(outputs).ToList();
            }
            return new FunctionBlockCall(arguments, this.DataType, 0, this.Name);
        }

        public override bool IsFunctionBlockInstance
        {
            get { return true; }
        }

        private readonly FramePointer framePointer; // Offset to this functionblock
    }
}
