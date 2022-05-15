using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using QUT.Gppg;

namespace STLang.AttributeStack
{
    public class SemanticStack
    {
        public SemanticStack(ErrorHandler error)
        {
            this.report = error;
            this.dataTypeList = new List<TypeNode>();
        }

        public void Push(TypeNode dataType)
        {
            this.dataTypeList.Add(dataType);
        }

        public void PushArrayElementType(LexLocation location)
        {
            int count = this.dataTypeList.Count;
            if (count == 0)
                this.dataTypeList.Add(TypeNode.Error);
            else
            {
                TypeNode dataType = this.dataTypeList[count - 1].BaseType;
                if (dataType.IsArrayType)
                {
                    ArrayType array = (ArrayType)dataType;
                    this.dataTypeList.Add(array.ElementType);
                }
                else
                {
                    this.report.SemanticError(85, dataType.Name, location);
                    this.dataTypeList.Add(TypeNode.Error);
                }
            }
        }

        public void PushFieldType(string identifier, LexLocation location)
        {
            if (this.dataTypeList.Count == 0)
            {
                string msg;
                msg = "SemanticStack.PushFieldType(): Semantic stack is empty";
                throw new STLangCompilerError(msg);
            }
            else
            {
                TypeNode dataType = this.dataTypeList.Last().BaseType;
                if (dataType.IsStructType)
                {
                    FieldSymbol fieldSymbol;
                    StructType structure = (StructType)dataType;
                    if (structure.LookUp(identifier, out fieldSymbol))
                        this.dataTypeList.Add(fieldSymbol.DataType);
                    else
                    {
                        this.report.SemanticError(4, identifier, structure.Name, location);
                        this.dataTypeList.Add(TypeNode.Error);
                    }
                }
                else if (!dataType.IsFunctionBlockType)
                    this.dataTypeList.Add(TypeNode.Error);
                else
                {
                    InstanceSymbol member;
                    FunctionBlockType functionBlock = (FunctionBlockType)dataType;

                    if (functionBlock.LookUp(identifier, out member))
                    {
                        if (member.VariableType == STVarType.VAR_INPUT || member.VariableType == STVarType.VAR_INOUT)
                            this.dataTypeList.Add(member.DataType);
                        else
                        {
                            // Error
                            this.dataTypeList.Add(TypeNode.Error);
                        }
                    }
                    else
                    {
                        this.report.SemanticError(136, identifier, functionBlock.Name, location);
                        this.dataTypeList.Add(TypeNode.Error);
                    }
                }
            }
        }

        public void CheckIfArrayType(LexLocation location)
        {
            int count = this.dataTypeList.Count;
            if (count == 0)
                throw new STLangCompilerError("Semantic stack is empty");
            else if (count == 1)
            {
                this.report.SemanticError(122, location);
            }
            else
            {
                TypeNode dataType = this.dataTypeList[count - 2];
                if (dataType != TypeNode.Error && ! dataType.IsArrayType)
                    this.report.SemanticError(85, dataType.Name, location);
            }
        }

        public void CheckIfStructType(LexLocation location)
        {
            int count = this.dataTypeList.Count;
            if (count == 0)
                throw new STLangCompilerError("Semantic stack is empty");
            else if (count == 1)
                this.report.SemanticError(122, location);
            else
            {
                TypeNode dataType = this.dataTypeList[count - 1];
                if (dataType != TypeNode.Error && ! dataType.IsStructType)
                    this.report.SemanticError(65, dataType.Name, location);
            }
        }

        public TypeNode Pop()
        {
            int count = this.dataTypeList.Count;
            if (count == 0)
                throw new STLangCompilerError("Semantic stack is empty");
            else
            {
                TypeNode topElement = this.dataTypeList.Last();
                this.dataTypeList.RemoveAt(count - 1);
                return topElement;
            }
        }

        public TypeNode Top
        {
            get
            {
                int count = this.dataTypeList.Count;
                if (count == 0)
                    throw new STLangCompilerError("Semantic stack is empty");
                else
                    return this.dataTypeList[count - 1];
            }
        }

        public TypeNode Top2
        {
            get
            {
                int count = this.dataTypeList.Count;
                if (count == 0)
                    throw new STLangCompilerError("Semantic stack is empty");
                else if (count == 1)
                    return TypeNode.Error;
                else
                    return this.dataTypeList[count - 2];
            }
        }

        public void Clear()
        {
            this.dataTypeList.Clear();
        }

        public bool IsEmpty 
        { 
            get { return this.dataTypeList.Count == 0; } 
        }

        private readonly ErrorHandler report;

        private readonly List<TypeNode> dataTypeList;
    }
}
