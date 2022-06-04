using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using STLang.Parser;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.SymbolTable;
using STLang.ErrorManager;

namespace STLang.MemoryLayout
{
    public class RWMemoryLayoutManager
    {
        public RWMemoryLayoutManager()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.realIndex = 0;
            this.lrealIndex = 0;
            this.sintIndex = 0;
            this.intIndex = 0;
            this.dintIndex = 0;
            this.lintIndex = 0;
            this.stringIndex = 0;
            this.wstringIndex = 0;
            this.temporaryID = 0;
            this.realRegister = 0;
            this.lrealRegister = 0;
            this.sintRegister = 0;
            this.intRegister = 0;
            this.dintRegister = 0;
            this.lintRegister = 0;
            this.rwDataSegmentSize = 0;
        }

        public void SetSegmentAlignment(int alignment)
        {
            int remainder = this.rwDataSegmentSize % alignment;
            if (remainder > 0)
                this.rwDataSegmentSize += alignment - remainder;
        }

        private TypeNode MakeArrayType(TypeNode elementType, int elementCount)
        {
            string typeID;
            TypeNode arrayType;
           
            typeID = string.Format("[0..{0}]{1}", elementCount, elementType.TypeID);
            if (TypeNode.LookUpType(typeID, out arrayType))
                return arrayType;
            else {
                uint bytes;
                string typeName;
                Expression size;
                
                bytes = (uint)elementCount * elementType.Size;
                size = STLangParser.MakeIntConstant((long)bytes);
                typeName = string.Format("ARRAY[0..{0}] OF {1}", elementCount - 1, elementType.Name);
                return new ArrayType(typeName, 0, elementCount - 1, bytes, size, elementType, elementType, typeID);
            }
        }

        private MemoryLocation MakeElementaryLocation(TypeNode dataType, ref int index, int elementCount)
        {
            MemoryLocation location;

            location = new ElementaryLocation(index, dataType, elementCount);
            this.rwDataSegmentSize += elementCount * (int)dataType.Size;
            index += elementCount;
            return location;
        }

        private MemoryLocation MakeStringLocation(TypeNode dataType, int elementCount)
        {
            MemoryLocation location;

            location = new StringLocation(this.stringIndex, this.sintIndex, dataType, elementCount);
            int stringBufferSize = elementCount * (int)dataType.Size;
            int stringPointerSize = elementCount * (int)TypeNode.DInt.Size;
            this.rwDataSegmentSize += stringBufferSize + stringPointerSize;
            this.stringIndex += elementCount;
            this.sintIndex += stringBufferSize;
            return location;
        }

        private MemoryLocation MakeWStringLocation(TypeNode dataType, int elementCount)
        {
            MemoryLocation location;

            location = new StringLocation(this.wstringIndex, this.intIndex, dataType, elementCount);
            int wstringBufferSize = elementCount * (int)dataType.Size;
            int wstringPointerSize = elementCount * (int)TypeNode.DInt.Size;
            this.rwDataSegmentSize += wstringBufferSize + wstringPointerSize;
            this.wstringIndex += elementCount;
            this.intIndex += wstringBufferSize;
            return location;
        }

        private bool IsConstantVariable(STVarQualifier varQual)
        {
            return varQual == STVarQualifier.CONSTANT
                || varQual == STVarQualifier.CONSTANT_RETAIN;
        }

        public InstanceSymbol CreateSymbol(string name, TypeNode dataType, STVarType varType,
                              STVarQualifier varQual, STDeclQualifier edgeQual, Expression initialValue,
                              int variablePos, int elementCount = 1)
        {
            if (dataType.IsElementaryType || dataType.IsAnyStringType)
            {
                MemoryLocation index;
                if (this.IsConstantVariable(varQual) && initialValue.IsConstant && variablePos >= 0)
                    // Don't reserve memory in rw-segment for constant variables of elementary type
                    index = MemoryLocation.UndefinedLocation;
                else if (varType == STVarType.VAR_INOUT)
                    // Reserve memory for pointer
                    index = this.GenerateIndex(TypeNode.DInt, false);
                else
                {
                    bool doAllocReg = variablePos >= 0 && varType == STVarType.VAR;
                    index = this.GenerateIndex(dataType, doAllocReg, elementCount);
                }
                return new ElementaryVariableSymbol(name, dataType, varType, varQual, edgeQual, initialValue, index, variablePos);
            }
            else if (dataType.IsStructType)
            {
                string key;
                FieldSymbol field;
                Expression initializer;
                StructInitializer structInitializer;
                Dictionary<string, InstanceSymbol> members;
                InstanceSymbol member, prevMember, firstMember;

                member = null;
                prevMember = null;
                firstMember = null;
                if (initialValue is StructInitializer)
                    structInitializer = (StructInitializer)initialValue;
                else
                    structInitializer = (StructInitializer)dataType.DefaultValue;
                field = ((StructType)dataType.BaseType).FirstField;
                members = new Dictionary<string, InstanceSymbol>();
                while (field != null)
                {
                    prevMember = member;
                    initializer = structInitializer.GetFieldInitializer(field.Name);
                    member = this.CreateSymbol(field.Name, field.DataType, varType, varQual, edgeQual, initializer, -1, elementCount);
                    if (prevMember == null)
                        firstMember = member;
                    else
                        prevMember.Next = member;
                    key = field.Name.ToUpper();
                    members.Add(key, member);
                    field = field.Next;
                }
                return new StructInstanceSymbol(name, dataType, varType, varQual, edgeQual, initialValue, members, firstMember, variablePos);
            }
            else if (dataType.IsFunctionBlockType)
            {
                string key;
                FramePointer offset;
                Expression initializer;
                Dictionary<string, InstanceSymbol> members;
                FunctionBlockInitializer funcBlockInitializer;
                InstanceSymbol member, field, prevMember, firstMember;

                member = null;
                prevMember = null;
                firstMember = null;
                offset = this.GetCurrentFramePointer();
                members = new Dictionary<string, InstanceSymbol>();
                field = ((FunctionBlockType)dataType.BaseType).FirstMember;
                if (initialValue is FunctionBlockInitializer)
                    funcBlockInitializer = (FunctionBlockInitializer)initialValue;
                else
                    funcBlockInitializer = (FunctionBlockInitializer)dataType.DefaultValue;
                while (field != null)
                {
                    prevMember = member;
                    initializer = funcBlockInitializer.GetInitializer(field.Name);
                    member = this.CreateSymbol(field.Name, field.DataType, varType, varQual, edgeQual, initializer, -1, elementCount);
                    if (prevMember == null)
                        firstMember = member;
                    else
                        prevMember.Next = member;
                    key = member.Name.ToUpper();
                    members.Add(key, member);
                    field = field.Next;
                }
                return new FunctionBlockInstanceSymbol(name, dataType, varType, varQual, edgeQual, initialValue, members, firstMember, variablePos, offset);
            }
            else if (dataType.IsArrayType)
            {
                ArrayType array = (ArrayType)dataType.BaseType;
                int elemCount = array.Range * elementCount;
                TypeNode elemType = array.ElementType;

                while (elemType.IsArrayType)
                {
                    array = (ArrayType)elemType;
                    elemCount *= array.Range;
                    elemType = array.ElementType;
                }
                InstanceSymbol elementSymbol;
                elementSymbol = this.CreateSymbol(name, elemType, varType, varQual, edgeQual, initialValue, -1, elemCount);
                return new ArrayInstanceSymbol(name, dataType, varType, varQual, edgeQual, initialValue, elementSymbol, variablePos);
            }
            else if (dataType == TypeNode.Error)
            {
                MemoryLocation index = MemoryLocation.UndefinedLocation;
                return new ElementaryVariableSymbol(name, dataType, varType, varQual, edgeQual, initialValue, index, variablePos);
            }
            string msg;
            msg = "CreateInstanceSymbol(): Unknown data type " + dataType.Name;
            throw new STLangCompilerError(msg);
        }

        private MemoryLocation GenerateIndex(TypeNode dataType, bool doAllocateRegister, int elementCount = 1)
        {
            if (dataType == null)
                throw new STLangCompilerError("GenerateIndex() failed: dataType is null");
            else if (dataType == TypeNode.LReal)
            {
                if (this.lrealRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                    return new RegisterVariable(this.lrealRegister++, dataType);
                else
                    return this.MakeElementaryLocation(dataType, ref this.lrealIndex, elementCount);
            }
            else if (dataType == TypeNode.Real)
            {
                if (this.realRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                    return new RegisterVariable(this.realRegister++, dataType);
                else
                    return this.MakeElementaryLocation(dataType, ref this.realIndex, elementCount);
            }
            else if (dataType.IsDateTimeType)
            {
                if (this.lintRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                    return new RegisterVariable(this.lintRegister++, dataType);
                else
                    return this.MakeElementaryLocation(dataType, ref this.lintIndex, elementCount);
            }
            else if (dataType.IsOrdinalType)
            {
                if (dataType.Size == 1)
                {
                    if (this.sintRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                        return new RegisterVariable(this.sintRegister++, dataType);
                    else
                        return this.MakeElementaryLocation(dataType, ref this.sintIndex, elementCount);
                }
                else if (dataType.Size == 2)
                {
                    if (this.intRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                        return new RegisterVariable(this.intRegister++, dataType);
                    else
                        return this.MakeElementaryLocation(dataType, ref this.intIndex, elementCount);
                }
                else if (dataType.Size == 4)
                {
                    if (this.dintRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                        return new RegisterVariable(this.dintRegister++, dataType);
                    else
                        return this.MakeElementaryLocation(dataType, ref this.dintIndex, elementCount);
                }
                else if (this.lintRegister < MAX_REGISTER_COUNT && doAllocateRegister)
                    return new RegisterVariable(this.lintRegister++, dataType);
                else
                    return this.MakeElementaryLocation(dataType, ref this.lintIndex, elementCount);
            }
            else if (dataType.IsStringType)
                return this.MakeStringLocation(dataType, elementCount);
            else if (dataType.IsWStringType)
                return this.MakeWStringLocation(dataType, elementCount);
            else
            {
                string msg;
                msg = "GenerateIndex() failed for datatype " + dataType.Name;
                throw new STLangCompilerError(msg);
            }
        }

        private FramePointer GetCurrentFramePointer()
        {
            FramePointer fp = new FramePointer();
            fp.RealBase = this.realIndex;
            fp.LRealBase = this.lrealIndex;
            fp.SIntBase = this.sintIndex;
            fp.IntBase = this.intIndex;
            fp.DIntBase = this.dintIndex;
            fp.LIntBase = this.lintIndex;
            fp.StringBase = this.stringIndex;
            fp.WStringBase = this.wstringIndex;
            return fp;
        } 

        public MemoryObject GenerateTemporary(Expression expr)
        {
            if (expr == null)
                throw new STLangCompilerError("GenerateTemporary() failed: expression is null");
            else
                return this.GenerateTemporary(expr.DataType);
        }

        public MemoryObject GenerateTemporary(TypeNode dataType)
        {
            Expression initValue = dataType.DefaultValue;
            ElementaryVariableSymbol tempSymbol;
            MemoryLocation location = this.GenerateIndex(dataType, false);
            string name = "#TEMP" + this.temporaryID++ + "#";

            tempSymbol = new ElementaryVariableSymbol(name, dataType, STVarType.VAR, STVarQualifier.NONE, STDeclQualifier.NONE, initValue, location, -1);
            return new MemoryObject(tempSymbol);
        }

        public void SetAbsoluteAddress(STLangSymbol symbol)
        {
            if (symbol.IsInstanceSymbol)
            {
                InstanceSymbol instance = (InstanceSymbol)symbol;
                if (instance.Location.AbsoluteAddress == null)
                    instance.Location.AbsoluteAddress = this.GetAbsoluteAddress(instance);
            }
        }

        public Expression GetAbsoluteAddress(InstanceSymbol symbol)
        {
            TypeNode dataType = symbol.DataType;
            int index = symbol.Location.Index;
           
            if (dataType == TypeNode.LReal)
                return STLangParser.MakeIntConstant(index * 8);
            else if (dataType == TypeNode.LInt || dataType == TypeNode.ULInt)
                return STLangParser.MakeIntConstant((this.lrealIndex + index) * 8);
            else if (dataType == TypeNode.LWord)
                return STLangParser.MakeIntConstant((this.lrealIndex + index) * 8);
            else if (dataType.IsDateTimeType)
                return STLangParser.MakeIntConstant((this.lrealIndex + index) * 8);
            else if (dataType == TypeNode.Real)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (index * 4));
            else if (dataType == TypeNode.DInt || dataType == TypeNode.UDInt)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + index) * 4);
            else if (dataType == TypeNode.DWord)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + index) * 4);
            else if (dataType == TypeNode.Word || dataType.IsEnumeratedType)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex) * 4 + index * 2);
            else if (dataType.IsWStringType)
            {
                StringLocation stringLoc = (StringLocation)symbol.Location;
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex + this.stringIndex + this.wstringIndex) * 4 + stringLoc.BufferOffset * 2);
            }
            else if (dataType.IsStringType)
            {
                StringLocation stringLoc = (StringLocation)symbol.Location;
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex + this.wstringIndex + this.stringIndex) * 4 + this.intIndex * 2 + stringLoc.BufferOffset);
            }
            else if (dataType == TypeNode.Int || dataType == TypeNode.UInt)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex + this.wstringIndex + this.stringIndex) * 4 + index * 2);
            else if (dataType == TypeNode.Byte || dataType == TypeNode.Bool)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex + this.wstringIndex + this.stringIndex) * 4 + this.intIndex * 2 + index);
            else if (dataType == TypeNode.SInt || dataType == TypeNode.USInt)
                return STLangParser.MakeIntConstant((this.lrealIndex + this.lintIndex) * 8 + (this.realIndex + this.dintIndex + this.wstringIndex + this.stringIndex) * 4 + this.intIndex * 2 + index);
            else if (dataType.IsArrayType)
            {
                if (!symbol.IsArrayInstance)
                {
                    string msg;
                    msg = "GetAbsoluteAddress(): ArrayInstanceSymbol expected.";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    ArrayInstanceSymbol array = (ArrayInstanceSymbol)symbol;
                    return this.GetAbsoluteAddress(array.ElementSymbol);
                }
            }
            else if (dataType.IsStructType)
            {
                if (!symbol.IsStructInstance)
                {
                    string msg;
                    msg = "GetAbsoluteAddress(): StructInstanceSymbol expected.";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    Expression absoluteAddress;
                    StructInstanceSymbol structure;
                    structure = (StructInstanceSymbol)symbol;
                    InstanceSymbol member = structure.FirstMember;

                    while (member != null)
                    {
                        absoluteAddress = this.GetAbsoluteAddress(member);
                        member.Location.AbsoluteAddress = absoluteAddress;
                        member = member.Next;
                    }
                    return STLangParser.MakeIntConstant(-1);
                }
            }
            else if (dataType.IsFunctionBlockType)
            {
                if (!symbol.IsFunctionBlockInstance)
                {
                    string msg;
                    msg = "GetAbsoluteAddress(): FunctionBlockInstanceSymbol expected.";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    Expression absoluteAddress;
                    FunctionBlockInstanceSymbol functionBlock;
                    functionBlock = (FunctionBlockInstanceSymbol)symbol;
                    InstanceSymbol member = functionBlock.FirstMember;

                    while (member != null)
                    {
                        absoluteAddress = this.GetAbsoluteAddress(member);
                        member.Location.AbsoluteAddress = absoluteAddress;
                        member = member.Next;
                    }
                    return STLangParser.MakeIntConstant(-1);
                }
            }
            throw new STLangCompilerError("GetAbsoluteAddress() failed for datatype " + dataType.Name);
        }

        public int RealCount
        {
            get { return this.realIndex; }
        }

        public int LRealCount
        {
            get { return this.lrealIndex; }
        }

        public int SIntCount
        {
            get { return this.sintIndex; }
        }

        public int IntCount
        {
            get { return this.intIndex; }
        }

        public int DIntCount
        {
            get { return this.dintIndex; }
        }

        public int LIntCount
        {
            get { return this.lintIndex; }
        }

        public int StringCount
        {
            get { return this.stringIndex; }
        }

        public int WStringCount
        {
            get { return this.wstringIndex; }
        }

        public int RWDataSegmentSize
        {
            get { return this.rwDataSegmentSize; }
        }

        private int rwDataSegmentSize;

        private int realIndex;

        private int lrealIndex;

        private int sintIndex;

        private int intIndex;

        private int dintIndex;

        private int lintIndex;

        private int stringIndex;

        private int wstringIndex;

        private int realRegister;

        private int lrealRegister;

        private int sintRegister;

        private int intRegister;

        private int dintRegister;

        private int lintRegister;

        private int temporaryID;

        private const int MAX_REGISTER_COUNT = 4;
    }

    public struct FramePointer
    {
        public int RealBase { get; set; }

        public int LRealBase { get; set; }

        public int DIntBase { get; set; }

        public int LIntBase { get; set; }

        public int SIntBase { get; set; }

        public int IntBase { get; set; }

        public int StringBase { get; set; }

        public int WStringBase { get; set; }
    }
}
