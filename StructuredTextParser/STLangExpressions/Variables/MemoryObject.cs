using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Parser;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.MemoryLayout;
using STLang.ErrorManager;
using STLang.VMInstructions;
using STLang.Statements;
using StructuredTextParser.Properties;

namespace STLang.Expressions
{
    public class MemoryObject : Expression
    {
        public MemoryObject(InstanceSymbol varSymbol)
            : base(varSymbol.DataType, varSymbol.Name)
        {
            this.offset = null;
            this.symbol = varSymbol;
            this.location = varSymbol.Location;
            this.isSimpleVariable = true;
            this.Length = 1;
            this.isNegated = false;
            this.initialValue = symbol.InitialValue;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                                 || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                                 || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public MemoryObject(InstanceSymbol symbol, Expression offset, string stringValue, int length)
            : base(symbol.DataType, stringValue)
        {
            this.offset = offset;
            this.symbol = symbol;
            this.location = symbol.Location;
            this.isSimpleVariable = false;
            this.Length = length;
            this.isNegated = false;
            this.initialValue = symbol.InitialValue;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                               || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                               || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public MemoryObject(Expression rValue, string lValueText, InstanceSymbol symbol, int length)
            : base(rValue.DataType, lValueText)
        {
            this.offset = null;
            this.symbol = symbol;
            this.location = symbol.Location;
            this.isSimpleVariable = false;
            this.Length = length;
            this.isNegated = false;
            this.initialValue = rValue;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                               || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                               || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public MemoryObject(string name, MemoryLocation index, TypeNode dataType, InstanceSymbol varSymbol, int length = 1)
            : base(dataType, name)
        {
            this.offset = null;
            this.symbol = varSymbol;
            this.location = index;
            this.isSimpleVariable = true;
            this.Length = length;
            this.isNegated = false;
            this.initialValue = symbol.InitialValue;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                                 || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                                 || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public MemoryObject(string name, MemoryLocation index, Expression offset, TypeNode dataType, InstanceSymbol symbol, int length = 1)
            : base(dataType, name)
        {
            this.offset = offset;
            this.symbol = symbol;
            this.location = index;
            this.isSimpleVariable = false;
            this.Length = length;
            this.isNegated = false;
            this.initialValue = symbol.InitialValue;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                                 || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                                 || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public MemoryObject(string name, MemoryLocation index, TypeNode dataType, 
                            InstanceSymbol symbol, Expression initVal, int length = 1)
            : base(dataType, name)
        {
            this.offset = null;
            this.symbol = symbol;
            this.location = index;
            this.isSimpleVariable = false;
            this.Length = length;
            this.isNegated = false;
            this.initialValue = initVal;
            this.isCallByReference = symbol.VariableType == STVarType.VAR_INOUT;
            this.isConstantLValue = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                                 || this.symbol.VarQualifier == STVarQualifier.CONSTANT_RETAIN
                                 || this.symbol.VariableType == STVarType.VAR_INPUT;
            this.isConstant = this.symbol.VarQualifier == STVarQualifier.CONSTANT
                           && this.symbol.VariableType == STVarType.VAR
                           && this.symbol.InitialValue.IsConstant;
        }

        public override bool IsInverted
        {
            get;
            set;
        }

        public override bool IsLinear()
        {
            return this.symbol.IsForLoopCtrlVar;
        }

        public override MemoryLocation Location 
        { 
            get { return this.location; } 
        }

        public Expression Offset 
        { 
            get { return this.offset; }
        }

        public Expression InitialValue
        {
            get { return this.initialValue; }
        }

        public InstanceSymbol Symbol 
        { 
            get { return this.symbol; } 
        }

        public override bool IsLValue 
        { 
            get { return true; } 
        }

        public bool IsSimpleVariable
        {
            get { return this.isSimpleVariable; }
        }

        public override bool IsConstant
        {
            get { return this.isConstant && (this.offset == null || this.offset.IsConstant); }
        }

        public override bool IsConstantLValue
        {
            get { return this.isConstantLValue; }
        }

        public override bool ConstantForLoopBounds(List<ForLoopData> forLoopDataList)
        {
            if (this.IsConstant)
                return true;
            else if (!this.symbol.IsForLoopCtrlVar)
                return false;
            else
            {
                Predicate<ForLoopData> searchCond;
			    searchCond = forLoop => forLoop.ControlVariable.Symbol == this.Symbol;
                ForLoopData forLoopData = forLoopDataList.Find(searchCond);
                if (forLoopData == null)
                    throw new STLangCompilerError(Resources.EVALUATE, this.Name);
                else
                    return forLoopData.InitialValue.ConstantForLoopBounds(forLoopDataList)
                        && forLoopData.StopValue.ConstantForLoopBounds(forLoopDataList);
            }
        }

        public override int Evaluate(int bounds, List<ForLoopData> forLoopDataList)
        {
            if (!this.symbol.IsForLoopCtrlVar)
                throw new STLangCompilerError(Resources.EVALUATE, this.Name);
            else
            {
                Predicate<ForLoopData> searchCond;
			    searchCond = forLoop => forLoop.ControlVariable.Symbol == this.Symbol;
                int index = forLoopDataList.FindIndex(searchCond);
                if (index == -1)
                    throw new STLangCompilerError(Resources.EVALUATE2, this.Name);
                else
                {
                    Expression forLoopBound;
                    int bitMask = 1 << index;
                    if ((bitMask & bounds) == 0)
                        forLoopBound = forLoopDataList[index].InitialValue;
                    else
                        forLoopBound = forLoopDataList[index].StopValue;
                    return forLoopBound.Evaluate(bounds, forLoopDataList);
                }
            }
        }

        public override object Evaluate()
        {
            if (!this.isConstant)
                throw new STLangCompilerError("Can't evaluate nonconstant l-value.");
            else if (initialValue == null)
                throw new STLangCompilerError("Initial value is null.");
            else
                return this.initialValue.Evaluate();
        }

        public override void GenerateStore()
        {
            int index = this.Location.Index;
            if (this.offset != null)
            {
                this.offset.GenerateLoad();
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.IADD);
                }
                if (this.DataType == TypeNode.LReal)
                    this.StoreInstruction(VirtualMachineInstruction.DSTOX, index);
                else if (this.DataType == TypeNode.Real)
                    this.StoreInstruction(VirtualMachineInstruction.FSTOX, index);
                else if (this.DataType.IsDateTimeType)
                    this.StoreInstruction(VirtualMachineInstruction.LSTOX, index);
                else if (this.DataType.IsOrdinalType)
                {
                    uint size = this.DataType.Size;
                    if (size == 1)
                        this.StoreInstruction(VirtualMachineInstruction.BSTOX, index);
                    else if (size == 2)
                        this.StoreInstruction(VirtualMachineInstruction.ISTOX, index);
                    else if (size == 4)
                        this.StoreInstruction(VirtualMachineInstruction.WSTOX, index);
                    else
                        this.StoreInstruction(VirtualMachineInstruction.LSTOX, index);
                }
                else if (this.DataType.IsStringType)
                {
                    int bytes = (int)this.DataType.Size;
                    this.StoreInstruction(VirtualMachineInstruction.ICONST, bytes);
                    this.StoreInstruction(VirtualMachineInstruction.SSTOX, index);
                }
                else if (this.DataType.IsWStringType)
                {
                    int bytes = (int)this.DataType.Size;
                    this.StoreInstruction(VirtualMachineInstruction.ICONST, bytes);
                    this.StoreInstruction(VirtualMachineInstruction.WSSTOX, index);
                }
                else if (this.DataType.IsStructType)
                {
                    if (!(this.symbol.IsStructInstance))
                    {
                        string msg;
                        msg = "GenerateStore(): StructInstanceSymbol expected.";
                        throw new STLangCompilerError(msg);
                    }
                    else
                    {
                        InstanceSymbol member;
                        StructInstanceSymbol structure;
                        structure = (StructInstanceSymbol)this.symbol;
                        member = structure.FirstMember;
                        this.StoreMembersInReverse(member);
                    }
                }
                else if (this.DataType.IsArrayType)
                {
                    this.Location.AbsoluteAddress.GenerateLoad();
                    this.StoreInstruction(VirtualMachineInstruction.IADD);
                    this.StoreInstruction(VirtualMachineInstruction.MEM_COPY);
                }
                else if (this.DataType.IsFunctionBlockType)
                {
                    if (!this.symbol.IsFunctionBlockInstance)
                    {
                        string msg;
                        msg = "GenerateStore(): FunctionBlockInstanceSymbol expected.";
                        throw new STLangCompilerError(msg);
                    }
                    else
                    {
                        InstanceSymbol member;
                        FunctionBlockInstanceSymbol functionBlock;
                        functionBlock = (FunctionBlockInstanceSymbol)this.symbol;
                        member = functionBlock.FirstMember;
                        this.StoreMembersInReverse(member);
                    }
                }
            }
            else if (this.Location.ElementCount > 1)
            {
                this.Location.AbsoluteAddress.GenerateLoad();
                this.Location.Size.GenerateLoad();
                this.StoreInstruction(VirtualMachineInstruction.MEM_COPY);
            }
            else if (this.DataType == TypeNode.Real)
            {
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.FSTOX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO3);
            }
            else if (this.DataType == TypeNode.LReal)
            {
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.DSTOX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO3);
            }
            else if (this.DataType.IsOrdinalType)
            {
                uint size = this.DataType.Size;
                if (size == 1)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                        this.StoreInstruction(VirtualMachineInstruction.BSTOX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO3);
                }
                else if (size == 2)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                        this.StoreInstruction(VirtualMachineInstruction.ISTOX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO3);
                }
                else if (size == 4)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                        this.StoreInstruction(VirtualMachineInstruction.WSTOX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO3);
                }
                else if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.LSTOX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO3);
            }
            else if (this.DataType.IsStringType)
            {
                int bytes = (int)this.DataType.Size;
                this.StoreInstruction(VirtualMachineInstruction.ICONST, bytes);
                if (! this.isCallByReference)
                    this.StoreInstruction(VirtualMachineInstruction.SSTO, index);
                else {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.SSTOX, 0);
                }
            }
            else if (this.DataType.IsWStringType)
            {
                int bytes = (int)this.DataType.Size;
                this.StoreInstruction(VirtualMachineInstruction.ICONST, bytes);
                if (!this.isCallByReference)
                    this.StoreInstruction(VirtualMachineInstruction.WSSTO, index);
                else
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.WSTOX, 0);
                }
            }
        }

        private bool IsConstantDeclaration(InstanceSymbol symbol)
        {
            STVarQualifier varQual = this.symbol.VarQualifier;
            return varQual == STVarQualifier.CONSTANT
                || varQual == STVarQualifier.CONSTANT_RETAIN;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            int index = this.Location.Index;
            TypeNode dataType = this.DataType;
            STVarQualifier varQual = this.symbol.VarQualifier;
            if (dataType.IsElementaryType && IsConstantDeclaration(this.symbol) && this.offset == null)
                this.initialValue.GenerateLoad();
            else if (this.offset != null)
            {
                this.offset.GenerateLoad();
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.IADD);
                }
                if (dataType == TypeNode.LReal)
                    this.StoreInstruction(VirtualMachineInstruction.DLODX, index);
                else if (dataType == TypeNode.Real)
                    this.StoreInstruction(VirtualMachineInstruction.FLODX, index);
                else if (dataType.IsDateTimeType)
                    this.StoreInstruction(VirtualMachineInstruction.LLODX, index);
                else if (dataType.IsOrdinalType)
                {
                    uint size = dataType.Size;
                    if (size == 1)
                        this.StoreInstruction(VirtualMachineInstruction.BLODX, index);
                    else if (size == 2)
                        this.StoreInstruction(VirtualMachineInstruction.ILODX, index);
                    else if (size == 4)
                        this.StoreInstruction(VirtualMachineInstruction.WLODX, index);
                    else
                        this.StoreInstruction(VirtualMachineInstruction.LLODX, index);
                }
                else if (dataType.IsStringType)
                    this.StoreInstruction(VirtualMachineInstruction.SLODX, index);
                else if (dataType.IsWStringType)
                    this.StoreInstruction(VirtualMachineInstruction.WSLODX, index);
                else if (dataType.IsArrayType)
                {
                    if (!this.symbol.IsArrayInstance)
                    {
                        string msg;
                        msg = "MemoryObject.GenerateLoad(): ArrayInstanceSymbol expected";
                        throw new STLangCompilerError(msg);
                    }
                    else
                    {
                        ArrayInstanceSymbol array = (ArrayInstanceSymbol)this.symbol;
                        InstanceSymbol elementSymbol = array.ElementSymbol;
                        TypeNode elemDataType = elementSymbol.DataType;
                        if (elemDataType.IsElementaryType || elemDataType.IsTextType)
                        {
                            if (this.location.AbsoluteAddress == null)
                            {
                                string msg;
                                msg = "MemoryObject.GenerateLoad(): AbsoluteAddress is null.";
                                throw new STLangCompilerError(msg);
                            }
                            else
                            {
                                elementSymbol.Location.AbsoluteAddress.GenerateLoad();
                                this.StoreInstruction(VirtualMachineInstruction.IADD);
                            }
                        }
                    }
                }
            }
            else if (this.Location.ElementCount > 1)
                this.Location.AbsoluteAddress.GenerateLoad();
            else if (dataType == TypeNode.Real)
            {
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.FLODX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.FLOD, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.FLOD0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.FLOD1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.FLOD2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.FLOD3);
            }
            else if (dataType == TypeNode.LReal)
            {
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);  // push offset on stack
                    this.StoreInstruction(VirtualMachineInstruction.DLODX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.DLOD, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.DLOD0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.DLOD1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.DLOD2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.DLOD3);
            }
            else if (dataType.IsDateTimeType)
            {
                if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);  // push offset on stack
                    this.StoreInstruction(VirtualMachineInstruction.LLODX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD3);
            }
            else if (dataType.IsOrdinalType)
            {
                uint size = this.DataType.Size;
                if (size == 1)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);  // push offset on stack
                        this.StoreInstruction(VirtualMachineInstruction.BLODX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.BLOD, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.BLOD0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.BLOD1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.BLOD2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.BLOD3);
                }
                else if (size == 2)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);  // push offset on stack
                        this.StoreInstruction(VirtualMachineInstruction.ILODX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.ILOD, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.ILOD0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.ILOD1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.ILOD2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.ILOD3);
                }
                else if (size == 4)
                {
                    if (this.isCallByReference)
                    {
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);  // push offset on stack
                        this.StoreInstruction(VirtualMachineInstruction.WLODX, 0);
                    }
                    else if (!this.location.IsRegisterVariable)
                        this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    else if (index == 0)
                        this.StoreInstruction(VirtualMachineInstruction.WLOD0);
                    else if (index == 1)
                        this.StoreInstruction(VirtualMachineInstruction.WLOD1);
                    else if (index == 2)
                        this.StoreInstruction(VirtualMachineInstruction.WLOD2);
                    else if (index == 3)
                        this.StoreInstruction(VirtualMachineInstruction.WLOD3);
                }
                else if (this.isCallByReference)
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index); // push offset on stack
                    this.StoreInstruction(VirtualMachineInstruction.LLODX, 0);
                }
                else if (!this.location.IsRegisterVariable)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD, index);
                else if (index == 0)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD0);
                else if (index == 1)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD1);
                else if (index == 2)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD2);
                else if (index == 3)
                    this.StoreInstruction(VirtualMachineInstruction.LLOD3);
            }
            else if (dataType.IsStringType)
            {
                if (!this.isCallByReference)
                    this.StoreInstruction(VirtualMachineInstruction.SLOD, index);
                else
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.SLODX, 0);
                }
            }
            else if (dataType.IsWStringType)
            {
                if (!this.isCallByReference)
                    this.StoreInstruction(VirtualMachineInstruction.WSLOD, index);
                else
                {
                    this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
                    this.StoreInstruction(VirtualMachineInstruction.WSLODX, 0);
                }
            }
            else
                throw new STLangCompilerError(Resources.GENLOADTYPE, dataType.Name);
        }

        //private void GenerateLoad(InstanceSymbol member)
        //{
        //    TypeNode dataType = member.DataType;
        //    int index = member.Location.Index;
    
        //    if (dataType == TypeNode.LReal)
        //    {
        //        if (!this.isCallByReference)
        //            this.StoreInstruction(VirtualMachineInstruction.DLOD, index);
        //        else {
        //            this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
        //            this.StoreInstruction(VirtualMachineInstruction.DLODX, 0);
        //        }
        //    }
        //    else if (dataType == TypeNode.Real)
        //    {
        //        if (!this.isCallByReference)
        //            this.StoreInstruction(VirtualMachineInstruction.FLOD, index);
        //        else
        //        {
        //            this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
        //            this.StoreInstruction(VirtualMachineInstruction.FLODX, 0);
        //        }
        //    }
        //    else if (dataType.IsDateTimeType)
        //    {
        //        if (!this.isCallByReference)
        //            this.StoreInstruction(VirtualMachineInstruction.LLOD, index);
        //        else
        //        {
        //            this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
        //            this.StoreInstruction(VirtualMachineInstruction.LLODX, 0);
        //        }
        //    }
        //    else if (dataType.IsOrdinalType)
        //    {
        //        uint size = dataType.Size;
        //        if (!this.isCallByReference)
        //        {
        //            if (size == 1)
        //                this.StoreInstruction(VirtualMachineInstruction.BLOD, index);
        //            else if (size == 2)
        //                this.StoreInstruction(VirtualMachineInstruction.ILOD, index);
        //            else if (size == 4)
        //                this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
        //            else
        //                this.StoreInstruction(VirtualMachineInstruction.LLOD, index);
        //        }
        //        else
        //        {
        //            this.StoreInstruction(VirtualMachineInstruction.WLOD, index);
        //            if (size == 1)
        //                this.StoreInstruction(VirtualMachineInstruction.BLODX);
        //            else if (size == 2)
        //                this.StoreInstruction(VirtualMachineInstruction.ILODX);
        //            else if (size == 4)
        //                this.StoreInstruction(VirtualMachineInstruction.WLODX);
        //            else
        //                this.StoreInstruction(VirtualMachineInstruction.LLODX);
        //        }
        //    }
        //    else if (dataType.IsArrayType)
        //    {
        //        if (!member.IsArrayInstance)
        //        {
        //            string msg;
        //            msg = "MemoryObject.GenerateLoad(): Member must be an array.";
        //            throw new STLangCompilerError(msg);
        //        }
        //        else
        //        {
        //            InstanceSymbol arrayElement;
        //            arrayElement = ((ArrayInstanceSymbol)member).ElementSymbol;
        //            this.GenerateLoad(member);
        //        }
        //    }
        //    else if (dataType.IsStructType)
        //    {
        //        if (! member.IsStructInstance)
        //        {
        //            string msg;
        //            msg = "MemoryObject.GenerateLoad(): Member must be a structure.";
        //            throw new STLangCompilerError(msg);
        //        }
        //        else
        //        {
        //            member = ((StructInstanceSymbol)member).FirstMember;
        //            while (member != null)
        //            {
        //                this.GenerateLoad(member);
        //                member = member.Next;
        //            }
        //        }
        //    }
        //    else if (dataType.IsFunctionBlockType)
        //    {
        //        if (!member.IsFunctionBlockInstance)
        //        {
        //            string msg;
        //            msg = "MemoryObject.GenerateLoad(): Member must be a function block.";
        //            throw new STLangCompilerError(msg);
        //        }
        //        else
        //        {
        //            member = ((FunctionBlockInstanceSymbol)member).FirstMember;
        //            while (member != null)
        //            {
        //                this.GenerateLoad(member);
        //                member = member.Next;
        //            }
        //        }
        //    }
        //}

        private void GenerateStore(InstanceSymbol member)
        {
            TypeNode dataType = member.DataType;
            if (dataType.IsElementaryType || dataType.IsTextType)
            {
                int index = member.Location.Index;
                if (dataType == TypeNode.LReal)
                    this.StoreInstruction(VirtualMachineInstruction.DSTO, index);
                else if (dataType == TypeNode.Real)
                    this.StoreInstruction(VirtualMachineInstruction.FSTO, index);
                else if (dataType.IsDateTimeType)
                    this.StoreInstruction(VirtualMachineInstruction.LSTO, index);
                else if (dataType.IsOrdinalType)
                {
                    uint size = this.DataType.Size;
                    if (size == 1)
                        this.StoreInstruction(VirtualMachineInstruction.BSTO, index);
                    else if (size == 2)
                        this.StoreInstruction(VirtualMachineInstruction.ISTO, index);
                    else if (size == 4)
                        this.StoreInstruction(VirtualMachineInstruction.WSTO, index);
                    else
                        this.StoreInstruction(VirtualMachineInstruction.LSTO, index);
                }
                else if (dataType.IsStringType)
                    this.StoreInstruction(VirtualMachineInstruction.SSTO, index);
                else if (dataType.IsWStringType)
                    this.StoreInstruction(VirtualMachineInstruction.WSLOD, index);
                else
                {
                    string msg;
                    msg = "GenerateStore(member): Invalid member data type ";
                    throw new STLangCompilerError(msg + dataType.Name);
                }
                    
            }
            else if (dataType.IsArrayType)
            {

            }
            else if (dataType.IsStructType)
            {
                if (!(member.IsStructInstance))
                {

                }
                else
                {

                }
            }
            else if (dataType.IsFunctionBlockType)
            {

            }
            else
            {
                string msg = "GenerateStore(member): Invalid member data type ";
                throw new STLangCompilerError(msg + dataType.Name);
            }
        }

        private void StoreMembersInReverse(InstanceSymbol member)
        {
            if (member != null)
            {
                this.StoreMembersInReverse(member.Next);
                this.GenerateStore(member);
            }
        }

        public override void GenerateBoolExpression(List<int> trueBranch, List<int> falseBranch)
        {
            this.GenerateLoad();
            if (this.IsInverted)
            {
                trueBranch.Add(ILC);
                if (this.isNegated)
                    this.StoreInstruction(VirtualMachineInstruction.IJNEZ);
                else
                    this.StoreInstruction(VirtualMachineInstruction.IJEQZ);
            }
            else
            {
                falseBranch.Add(ILC);
                if (this.isNegated)
                    this.StoreInstruction(VirtualMachineInstruction.IJEQZ);
                else
                    this.StoreInstruction(VirtualMachineInstruction.IJNEZ);
            }
           
        }

        public override string ToString()
        {
            string stringVal = base.ToString();
            if (this.isNegated)
                return "NOT " + stringVal;
            return stringVal;
        }

        public override Expression InvertRelation(bool doInvert)
        {
            this.IsInverted = doInvert;
            return this;
        }

        public override Expression DeMorgan()
        {
            this.isNegated = !this.isNegated;
            return this;
        }

        public override bool IsCompoundExpression
        {
            get { return this.offset != null; }
        }

        private readonly Expression initialValue;

        private readonly MemoryLocation location;

        private readonly Expression offset;

        private readonly bool isSimpleVariable;

        private readonly InstanceSymbol symbol;

        private readonly bool isConstantLValue;

        private readonly bool isCallByReference;

        private readonly bool isConstant;

        private bool isNegated;
    }
}
