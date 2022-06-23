using QUT.Gppg;
using STLang.ConstantTokens;
using STLang.DataTypes;
using STLang.ErrorManager;
using STLang.Expressions;
using STLang.MemoryLayout;
using STLang.Statements;
using STLang.Symbols;
using STLang.VMInstructions;
using System;
using System.Collections.Generic;


namespace STLang.SymbolTable
{
    public class STLangSymbolTable
    {
        public STLangSymbolTable(ErrorHandler errorHandler)
        {
            this.scopeLevel = SYSTEM_LEVEL;
            this.report = errorHandler;
            this.currentFunction = null;
            this.currentFunctionBlock = null;
            this.userDefinedPOUList = new List<ProgramOrganizationUnitSymbol>();
        }

        static STLangSymbolTable()
        {
            symbolDictionary = new Dictionary<string, STLangSymbol>[MAX_LEXICAL_SCOPES];
            symbolDictionary[SYSTEM_LEVEL] = new Dictionary<string, STLangSymbol>();
            symbolDictionary[USER_LEVEL] = new Dictionary<string, STLangSymbol>();
            symbolDictionary[LOCAL_LEVEL] = new Dictionary<string, STLangSymbol>();
            Initialize();
        }

        public void ReInitialize()
        {
            symbolDictionary[LOCAL_LEVEL].Clear();
            UserDefinedFunctionSymbol.ReInitialize();
        }

        public void Push()
        {
            if (this.scopeLevel < MAX_LEXICAL_SCOPES)
                this.scopeLevel++;
            else
                throw new STLangCompilerError("Symbol table stack overflow.");
        }

        public void Pop()
        {
            if (this.scopeLevel < 0)
                throw new STLangCompilerError("Symbol table stack underflow.");
            else
            {
                symbolDictionary[this.scopeLevel].Clear();
                this.scopeLevel--;
            }
        }

        public IEnumerable<ProgramOrganizationUnitSymbol> ProgramOrganizationUnits
        {
            get { return this.userDefinedPOUList; }
        }

        public bool IsCurrentFunction(Expression expression)
        {
            if (!(expression is FunctionName))
                return false;
            else
                return ((FunctionName)expression).FunctionSymbol == this.currentFunction;
        }

        public bool IsRecursiveCall(Expression expression)
        {
            if (!(expression is UserDefinedFunctionCall))
                return false;
            else
                return ((UserDefinedFunctionCall)expression).UserDefinedFunctionSymbol == this.currentFunction;
        }

        public void InstallUserDefinedFunction(string name, TypeNode returnType, params TypeNode[] formalTypes)
        {
        }

        public TypeNode InstallUndefinedType(string typeName)
        {
            TypeNode undefinedType = new UndefinedType(typeName);
            this.InstallDerivedType(typeName, undefinedType, Expression.Error);
            return undefinedType;
        }

        public STLangSymbol InstallUndefinedFunction(string name, int argCount)
        {
            UndefinedFunctionSymbol undefFunction = new UndefinedFunctionSymbol(name);
            symbolDictionary[USER_LEVEL][name] = undefFunction;
            return undefFunction;
        }

        public UndefinedFunctionBlockSymbol InstallUndefinedFunctionBlock(string name)
        {
            UndefinedFunctionBlockSymbol undefFunctionBlock;
            undefFunctionBlock = new UndefinedFunctionBlockSymbol(name);
            symbolDictionary[USER_LEVEL][name.ToUpper()] = undefFunctionBlock;
            return undefFunctionBlock;
        }

        private static void InstallTypeConvFunction(TypeNode fromDataType, TypeNode toDataType)
        {
            string name = fromDataType.Name + "_TO_" + toDataType.Name;
            string name2 = "TO_" + toDataType.Name;                      // Overloaded function name
            InstallTypeConverterFunction(name, toDataType, fromDataType);
            InstallTypeConverterFunction(name2, toDataType, fromDataType);
        }

        public static void InstallStandardFunction(string name, TypeNode returnType, 
                                                   StandardLibraryFunction opCode, params TypeNode[] inputTypes)
        {
            string key = name.ToUpper();
            StandardFunctionSymbol standardFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                standardFunction = new StandardFunctionSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = standardFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is StandardFunctionSymbol)
                    standardFunction = (StandardFunctionSymbol)symbol;
                else
                {
                    string msg;
                    msg = string.Format("Symbolen {0} redan deklarerad som {1}", name, symbol.TypeName);
                    throw new STLangCompilerError(msg);
                }
            }
            standardFunction.Add(opCode, returnType, 0, inputTypes);
        }

        public static void InstallStandardFunction(string name, TypeNode returnType, StandardLibraryFunction opCode, 
                                                   int fixedInputTypeCount, params TypeNode[] inputTypes)
        {
            string key = name.ToUpper();
            StandardFunctionSymbol standardFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                standardFunction = new StandardFunctionSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = standardFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is StandardFunctionSymbol)
                    standardFunction = (StandardFunctionSymbol)symbol;
                else
                {
                    string msg;
                    msg = string.Format("Symbolen {0} redan deklarerad som {1}", name, symbol.TypeName);
                    throw new STLangCompilerError(msg);
                }
            }
            standardFunction.Add(opCode, returnType, fixedInputTypeCount, inputTypes);
        }

        public static void InstallStandardFunction(string name, TypeNode returnType, VirtualMachineInstruction opCode, params TypeNode[] inputTypes)
        {
            string key = name.ToUpper();
            VirtualMachineInstructionSymbol standardFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                standardFunction = new VirtualMachineInstructionSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = standardFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is VirtualMachineInstructionSymbol)
                    standardFunction = (VirtualMachineInstructionSymbol)symbol;
                else
                {
                    string msg;
                    msg = string.Format("Symbolen {0} redan deklarerad som {1}", name, symbol.TypeName);
                    throw new STLangCompilerError(msg);
                }
            }
            standardFunction.Add(opCode, returnType, inputTypes);
        }

        public static void InstallBitShiftFunction(string name, TypeNode returnType, VirtualMachineInstruction opCode, params TypeNode[] formalTypes)
        {
            string key = name.ToUpper();
            VirtualMachineInstructionSymbol standardFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                standardFunction = new BitShiftFunctionSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = standardFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is VirtualMachineInstructionSymbol)
                    standardFunction = (VirtualMachineInstructionSymbol)symbol;
                else
                {
                    string msg;
                    msg = string.Format("Symbolen {0} redan deklarerad som {1}", name, symbol.TypeName);
                    throw new STLangCompilerError(msg);
                }
            }
            standardFunction.Add(opCode, returnType, formalTypes);
        }

        public static void InstallExtensibleFunction(string name, TypeNode returnType, StandardLibraryFunction opCode,params TypeNode[] formalTypes)
        {
            int fixedInputTypeCount = 0;
            string key = name.ToUpper();
            ExtensibleFunctionSymbol extensibleFunction = null;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                switch (key)
                {
                    case "MUX":
                        fixedInputTypeCount = 1;
                        extensibleFunction = new MUXFunctionSymbol();
                        break;

                    case "MAX":
                        extensibleFunction = new MAXFunctionSymbol();
                        break;

                    case "MIN":
                        extensibleFunction = new MINFunctionSymbol();
                        break;

                    default:
                        {
                            string msg = "InstallExtStandardFunction(): ";
                            msg += "Unknown extensible function " + name;
                            throw new STLangCompilerError(msg);
                        }
                }
                symbolDictionary[SYSTEM_LEVEL][key] = extensibleFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                switch (key)
                {
                    case "MUX":
                        fixedInputTypeCount = 1;
                        break;

                    case "MAX":
                        break;

                    case "MIN":
                        break;

                    default:
                        {
                            string msg = "InstallExtStandardFunction(): ";
                            msg += "Unknown extensible function " + name;
                            throw new STLangCompilerError(msg);
                        }
                }
                if (symbol is ExtensibleFunctionSymbol)
                    extensibleFunction = (ExtensibleFunctionSymbol)symbol;
                else
                {
                    string format = "Symbolen {0} redan deklarerad som {1}";
                    string msg = string.Format(format, name, symbol.TypeName);
                    throw new STLangCompilerError(msg);
                }
            }
            extensibleFunction.Add(opCode, returnType, fixedInputTypeCount, formalTypes);
        }

        private static void InstallBCDConverterFunction(string name, TypeNode toType, TypeNode fromType)
        {
            string key = name.ToUpper();
            BCDConverterSymbol conversionFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                conversionFunction = new BCDConverterSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = conversionFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is BCDConverterSymbol)
                    conversionFunction = (BCDConverterSymbol)symbol;
                else
                {
                    string msg;
                    msg = "Symbolen " + name + " redan deklarerad som " + symbol.TypeName;
                    throw new STLangCompilerError(msg);
                }
            }
        }

        private static void InstallTypeConverterFunction(string name, TypeNode toType, TypeNode fromType)
        {
            string key = name.ToUpper();
            TypeConverterSymbol conversionFunction;
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                conversionFunction = new TypeConverterSymbol(name);
                symbolDictionary[SYSTEM_LEVEL][key] = conversionFunction;
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[SYSTEM_LEVEL][key];
                if (symbol is TypeConverterSymbol)
                    conversionFunction = (TypeConverterSymbol)symbol;
                else
                {
                    string msg;
                    msg = "Symbolen " + name + " redan deklarerad som " + symbol.TypeName;
                    throw new STLangCompilerError(msg);
                }
            }
            if (fromType.IsIntegerType && toType == TypeNode.LReal)
            {
                if (fromType.Size <= TypeNode.DInt.Size)
                    conversionFunction.Add(toType, fromType, VirtualMachineInstruction.I2D);
                else
                    conversionFunction.Add(toType, fromType, VirtualMachineInstruction.L2D);
            }
            else if (fromType == TypeNode.LReal && toType.IsIntegerType)
            {
                if (toType.Size <= TypeNode.DInt.Size)
                    conversionFunction.Add(toType, fromType, VirtualMachineInstruction.D2I);
                else 
                    conversionFunction.Add(toType, fromType, VirtualMachineInstruction.D2L);
            }
            else if (fromType.IsIntegerType && toType == TypeNode.Real)
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.I2F);
            else if (fromType == TypeNode.Real && toType.IsIntegerType)
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.F2I);
            else if (fromType == TypeNode.Real && toType == TypeNode.LReal)
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.F2D);
            else if (fromType == TypeNode.LReal && toType == TypeNode.Real)
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.D2F);
            else if (fromType == TypeNode.LReal && toType.IsBitStringType)
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.D2I);
            else
                conversionFunction.Add(toType, fromType, VirtualMachineInstruction.CONV);
            
        }

        public void InstallUndeclaredVariable(string name, out STLangSymbol varSym)
        {
            string key = name.ToUpper();
            varSym = new ElementaryVariableSymbol(name, TypeNode.Error, STVarType.VAR, 
                              STVarQualifier.NONE, STDeclQualifier.NONE, Expression.Error, -1);
            symbolDictionary[this.scopeLevel][key] = varSym;
        }

        public void InstallUndeclaredDirectVariable(TokenDirectVar directVarToken, out STLangSymbol symbol)
        {
            string name = directVarToken.ToString().ToUpper();
            DirectRepVarSymbol directVarSym = new DirectRepVarSymbol(directVarToken);
            symbolDictionary[this.scopeLevel][name] = directVarSym;
            symbol = directVarSym;
        }

        public void InstallLocalVariables(List<InstanceSymbol> symbolList)
        {
            string key;

            foreach (InstanceSymbol symbol in symbolList)
            {
                key = symbol.Name.ToUpper();
                symbolDictionary[this.scopeLevel][key] = symbol;
            }
        }

        public void SaveFunctionDefinition(StatementList functionBody, Dictionary<string, Expression> constants, RWMemoryLayoutManager rwMemManager)
        {
            if (this.currentFunction == null)
                throw new STLangCompilerError("Current function object is null");
            else
                this.currentFunction.SaveDefinition(functionBody, constants, rwMemManager);
        }

        public void SaveFunctionBlockBody(StatementList functionBlockBody, Dictionary<string, Expression> constants, RWMemoryLayoutManager rwMemManager)
        {
            if (this.currentFunctionBlock == null)
                throw new STLangCompilerError("Current function block object is null.");
            else
                this.currentFunctionBlock.SaveDefinition(functionBlockBody, constants, rwMemManager);
        }

        public void InstallDerivedType(string derivedTypeName, TypeNode baseDataType, Expression initValue)
        {
            string key;
            TypeNode derivedType;

            if (baseDataType.IsSubrangeType)
                derivedType = new DerivedType(derivedTypeName, baseDataType, initValue);
            else
                derivedType = new DerivedType(derivedTypeName, baseDataType.BaseType, initValue);
            if (baseDataType.IsEnumeratedType)
                this.InstallEnumeratedConstants(baseDataType, derivedTypeName);
            key = derivedTypeName.ToUpper();
            TypeNameSymbol typeNameSymbol = new TypeNameSymbol(derivedTypeName, derivedType);
            symbolDictionary[this.scopeLevel][key] = typeNameSymbol;
        }

        public void InstallNamedValue(string name, Expression value, TypeNode dataType, string typeName)
        {
            string key = typeName + "#" + name;

            key = key.ToUpper();
            if (! symbolDictionary[scopeLevel].ContainsKey(key)) {
                NamedValueSymbol namedValueSymbol;

                namedValueSymbol = new NamedValueSymbol(key, dataType, value);
                symbolDictionary[scopeLevel][key] = namedValueSymbol;
            }
        }

        public void InstallEnumeratedConstants(EnumeratedType enumType)
        {
            string key;
            ushort enumValue = 0;
            EnumSymbol enumSymbol;

            foreach (string enumConst in enumType.IdentifierList)
            {
                key = enumConst.ToUpper();
                enumSymbol = new EnumSymbol(enumConst, enumType, enumValue++);
                symbolDictionary[scopeLevel][key] = enumSymbol;
            }
        }

        public void InstallEnumeratedConstants(TypeNode derivedType, string typeName)
        {
            string key;
            ushort enumValue = 0;
            EnumSymbol enumSymbol;
            string qualifiedEnumConst;
            EnumeratedType enumType = (EnumeratedType)derivedType.BaseType;
        
            foreach (string enumConst in enumType.IdentifierList)
            {
                // Store qualified identifier in symbol table: typename#enumConst
                qualifiedEnumConst = typeName + "#" + enumConst;
                enumSymbol = new EnumSymbol(qualifiedEnumConst, derivedType, enumValue++);
                key = qualifiedEnumConst.ToUpper();
                symbolDictionary[scopeLevel][key] = enumSymbol;
            }
            EnumeratedType.AddTypeName(typeName);
        }

        public void InstallDirectVariable(string name, TypeNode dataType, STVarType varType, 
                          STVarQualifier varQual, STDeclQualifier edgeQual, char size,
                          char location, ushort[] address, out InstanceSymbol symbol)
        {
            string key = name.ToUpper();
            symbol = new DirectRepVarSymbol(name, dataType, varType, varQual, edgeQual, location, size, address);
            symbolDictionary[this.scopeLevel][key] = symbol;
        }

        public void InstallEnumValue(string enumName, TypeNode dataType, ushort enumValue)
        {
            string key = enumName.ToUpper();
            EnumSymbol enumSymbol = new EnumSymbol(enumName, dataType, enumValue);
            symbolDictionary[this.scopeLevel][key] = enumSymbol;
        }

        public void InstallFunctionProtoType(string name, TypeNode resultDataType, POUVarDeclarations varDecls, LexLocation loc)
        {
            string key = name.ToUpper();
           
            if (!symbolDictionary[USER_LEVEL].ContainsKey(key))
            {
                this.currentFunction = new UserDefinedFunctionSymbol(name);
                symbolDictionary[USER_LEVEL][key] = this.currentFunction;
                this.userDefinedPOUList.Add(this.currentFunction);
            }
            else
            {
                STLangSymbol symbol = symbolDictionary[USER_LEVEL][key];
                if (symbol is UserDefinedFunctionSymbol)
                    this.currentFunction = (UserDefinedFunctionSymbol)symbol; // Overloaded function
                else
                {
                    string msg = string.Format("Symbol {0} already defined.", name);
                    throw new STLangCompilerError(msg);
                }
            }
            this.currentFunction.Add(resultDataType, varDecls, loc);
        }

        public void InstallFunctionBlockProtoType(string name, FunctionBlockType dataType, POUVarDeclarations varDecls, LexLocation loc)
        {
            string key = name.ToUpper();
            if (!symbolDictionary[USER_LEVEL].ContainsKey(key))
            {
                this.currentFunctionBlock = new UserDefinedFunctionBlockSymbol(name, dataType, varDecls);
                symbolDictionary[USER_LEVEL][key] = currentFunctionBlock;
                this.userDefinedPOUList.Add(this.currentFunctionBlock);
            }
            else
            {
                string msg = string.Format("Symbol {0} already defined.", name);
                throw new STLangCompilerError(msg);
            }
        }

        private static void InstallStandardFunctionBlock(string name, FunctionBlockType dataType, List<InstanceSymbol> members, int id)
        {
            string key = name.ToUpper();
            if (!symbolDictionary[SYSTEM_LEVEL].ContainsKey(key))
            {
                StandardFunctionBlockSymbol fbSymbol;
                fbSymbol = new StandardFunctionBlockSymbol(name, dataType, members, id);
                symbolDictionary[SYSTEM_LEVEL][key] = fbSymbol;
            }
            else
            {
                string msg = string.Format("Symbol {0} already defined.", name);
                throw new STLangCompilerError(msg);
            }
        }

        private bool FindSymbol(string identifier, out STLangSymbol symbol, out int scopeLev)
        {
            string ident = identifier.ToUpper();
            for (int i = this.scopeLevel; i >= SYSTEM_LEVEL; i--)
            {
                if (symbolDictionary[i].ContainsKey(ident))
                {
                    symbol = symbolDictionary[i][ident];
                    scopeLev = i;
                    return true;
                }
            }
            string qualifiedName;
            foreach (string typeName in EnumeratedType.TypeNames)
            {
                qualifiedName = typeName + "#" + ident;
                qualifiedName = qualifiedName.ToUpper();
                for (int i = this.scopeLevel; i >= SYSTEM_LEVEL; i--)
                {
                    if (symbolDictionary[i].ContainsKey(qualifiedName))
                    {
                        symbol = symbolDictionary[i][qualifiedName];
                        scopeLev = i;
                        return true;
                    }
                }
            }
            foreach (string typeName in NamedValueType.TypeNames)
            {
                qualifiedName = typeName + "#" + ident;
                qualifiedName = qualifiedName.ToUpper();
                for (int i = this.scopeLevel; i >= SYSTEM_LEVEL; i--)
                {
                    if (symbolDictionary[i].ContainsKey(qualifiedName))
                    {
                        symbol = symbolDictionary[i][qualifiedName];
                        scopeLev = i;
                        return true;
                    }
                }
            }
            symbol = null;
            scopeLev = -1;
            return false;
        }

        public bool IsValidUserDefinedSymbol(string identifier, LexLocation location)
        {
            int scopeLev;
            STLangSymbol symbol;

            if (! this.FindSymbol(identifier, out symbol, out scopeLev))
                return true;
            else if (scopeLev == this.scopeLevel)
                // Error: Multiply declared identifier
                this.report.SemanticError(1, identifier, location);
            else if (symbol is ElementaryVariableSymbol)
                this.report.SemanticError(1, identifier, location);
            else if (symbol is TypeNameSymbol)
                // Error: Redefinition of type
                this.report.SemanticError(155, identifier, location);
            else if (symbol is EnumSymbol)
                // Error: Identifier is an enumerated constant
                this.report.SemanticError(-9, identifier, location);
            else if (symbol is StandardFunctionSymbol)
                // Error: Identifier is a standard function
                this.report.SemanticError(153, identifier, location);
            else if (symbol is ExtensibleFunctionSymbol)
                // Error: Identifier is an extensible standard function
                this.report.SemanticError(153, identifier, location);
            else if (symbol is StandardFunctionBlockSymbol)
                // Error: Identifier is a standard function block
                this.report.SemanticError(154, identifier, location);
            return false;
        }

        public bool IsValidTypedEnumSymbol(string enumName, LexLocation location)
        {
            int scopeLev;
            STLangSymbol symbol;

            if (!this.FindSymbol(enumName, out symbol, out scopeLev))
                return true;
            else if (symbol is EnumSymbol || symbol is ElementaryVariableSymbol)
                return true;
            else if (symbol is TypeNameSymbol)
                // Error: Redefinition of type
                this.report.SemanticError(155, enumName, location);
            else if (symbol is StandardFunctionSymbol)
                // Error: Identifier is a standard function
                this.report.SemanticError(153, enumName, location);
            else if (symbol is ExtensibleFunctionSymbol)
                // Error: Identifier is an extensible standard function
                this.report.SemanticError(153, enumName, location);
            else if (symbol is StandardFunctionBlockSymbol)
                // Error: Identifier is a standard function block
                this.report.SemanticError(154, enumName, location);
            return false;
        }

        public bool Lookup(string identifier, TypeNode subrangeType, out STLangSymbol subrangeSymbol, LexLocation location)
        {
            if (subrangeType.IsEnumeratedType)
            {
                string qualifiedValue = subrangeType.Name + "#" + identifier;

                qualifiedValue = qualifiedValue.ToUpper();
                for (int i = this.scopeLevel; i >= 0; i--)
                {
                    if (symbolDictionary[i].ContainsKey(qualifiedValue))
                    {
                        subrangeSymbol = symbolDictionary[i][qualifiedValue];
                        return true;
                    }
                }
            }
            identifier = identifier.ToUpper();
            for (int i = this.scopeLevel; i >= 0; i--)
            {
                if (symbolDictionary[i].ContainsKey(identifier))
                {
                    subrangeSymbol = symbolDictionary[i][identifier];
                    return true;
                }
            }
            subrangeSymbol = null;
            return false;
        }

        public bool Lookup(string name, out STLangSymbol symbol, LexLocation location)
        {
            symbol = null;
            string ident = name.ToUpper();
            for (int i = LOCAL_LEVEL; i >= SYSTEM_LEVEL; i--)
            { 
                if (symbolDictionary[i].ContainsKey(ident))
                {
                    symbol = symbolDictionary[i][ident];
                    return true;
                } 
            }
            // Check if name is an unqualified enumerated or named value constant

            string qualifiedName;
            List<TypeNode> uniqueEnumDataTypeList = new List<TypeNode>();
            
            foreach (string typeName in EnumeratedType.TypeNames)
            {
                qualifiedName = typeName + "#" + ident;
                qualifiedName = qualifiedName.ToUpper();
                for (int i = this.scopeLevel; i >= 0; i--)
                {
                    if (symbolDictionary[i].ContainsKey(qualifiedName))
                    {
                        STLangSymbol enumSymbol;
                        Predicate<TypeNode> equivalentTypes;
                        enumSymbol = symbolDictionary[i][qualifiedName];
                        equivalentTypes = dt => dt == enumSymbol.DataType;
                        if (!uniqueEnumDataTypeList.Exists(equivalentTypes))
                            uniqueEnumDataTypeList.Add(enumSymbol.DataType);
                        if (symbol == null)
                            symbol = enumSymbol;
                    }
                }
            }
            List<TypeNode> uniqueNamedValueDataTypeList = new List<TypeNode>();
            foreach (string typeName in NamedValueType.TypeNames)
            {
                qualifiedName = typeName + "#" + ident;
                qualifiedName = qualifiedName.ToUpper();
                for (int i = this.scopeLevel; i >= SYSTEM_LEVEL; i--)
                {
                    if (symbolDictionary[i].ContainsKey(qualifiedName))
                    {
                        STLangSymbol namedValueSymbol;
                        Predicate<TypeNode> equivalentTypes;

                        namedValueSymbol = symbolDictionary[i][qualifiedName];
                        equivalentTypes = dt => dt == namedValueSymbol.DataType;
                        if (!uniqueNamedValueDataTypeList.Exists(equivalentTypes))
                            uniqueNamedValueDataTypeList.Add(namedValueSymbol.DataType);
                        if (symbol == null)
                            symbol = namedValueSymbol;
                    }
                }
            }
            int uniqueDataTypeCount = uniqueEnumDataTypeList.Count 
                                    + uniqueNamedValueDataTypeList.Count;
            if (uniqueDataTypeCount == 0)
                return false; // Error. Undefined symbol
            else if (uniqueDataTypeCount == 1)
                return true; // Unique enumerated or named value symbol found
            else {
                // Error. 'name' is ambiguous
                if (location != null)
                    this.report.SemanticError(112, name, location);
                return true;
            }
        }

        static FunctionBlockType MakeStdFunctionBlockType(string name, List<InstanceSymbol> members)
        {
            uint bytes = 0;
            string typeID = "{" + name + "}";
            foreach (InstanceSymbol member in members)
            {
                bytes += member.DataType.Size;
            }
            Expression size = new UDIntConstant(bytes);
            return new FunctionBlockType(name, members, bytes, size, typeID);
        }

        static public InstanceSymbol MakeInputVariable(string name, TypeNode dataType, int position, 
                                                      STDeclQualifier edgeQual = STDeclQualifier.NONE)
        {
            Expression initialValue = dataType.DefaultValue;

            return new ElementaryVariableSymbol(name, dataType, STVarType.VAR_INPUT,
                            STVarQualifier.NONE, edgeQual, initialValue, position);
        }

        static private InstanceSymbol MakeOutputVariable(string name, TypeNode dataType)
        {
           Expression initialValue = dataType.DefaultValue;

           return new ElementaryVariableSymbol(name, dataType, STVarType.VAR_OUTPUT, 
                            STVarQualifier.NONE, STDeclQualifier.NONE, initialValue, -1);
        }

        static private InstanceSymbol MakeLocalVariable(string name, TypeNode dataType, STVarQualifier varQual, Expression initValue, int position)
        {
            return new ElementaryVariableSymbol(name, dataType, STVarType.VAR, varQual,
                             STDeclQualifier.NONE, initValue, position);
        }

        private static void Initialize()
        {
            List<TypeNode> allDataTypes = new List<TypeNode>
            {
                TypeNode.SInt, TypeNode.Int, TypeNode.DInt, TypeNode.LInt,
                TypeNode.USInt, TypeNode.UInt, TypeNode.UDInt, TypeNode.ULInt,
                TypeNode.Bool, TypeNode.Byte, TypeNode.Word, TypeNode.DWord, 
                TypeNode.LWord, TypeNode.Real, TypeNode.LReal, TypeNode.Time, 
                TypeNode.Date, TypeNode.DateAndTime, TypeNode.TimeOfDay, 
                TypeNode.String, TypeNode.WString
            };

            List<TypeNode> anySignedIntType = new List<TypeNode>
            { 
                TypeNode.SInt, TypeNode.Int, TypeNode.DInt, TypeNode.LInt
            };

            List<TypeNode> anyUnsignedIntType = new List<TypeNode>
            { 
                TypeNode.USInt, TypeNode.UInt, TypeNode.UDInt, TypeNode.ULInt
            };

            List<TypeNode> anyIntType = new List<TypeNode>(anySignedIntType);
            anyIntType.AddRange(anyUnsignedIntType);

            List<TypeNode> anyBitStringType = new List<TypeNode>
            {
                TypeNode.Bool, TypeNode.Byte, TypeNode.Word, TypeNode.DWord, TypeNode.LWord
            };

            // Standrad library numerical functions

            InstallStandardFunction("ABS", TypeNode.SInt, StandardLibraryFunction.IABS, TypeNode.SInt);
            InstallStandardFunction("ABS", TypeNode.Int, StandardLibraryFunction.IABS, TypeNode.Int);
            InstallStandardFunction("ABS", TypeNode.DInt, StandardLibraryFunction.IABS, TypeNode.DInt);
            InstallStandardFunction("ABS", TypeNode.LInt, StandardLibraryFunction.LABS, TypeNode.LInt);
            InstallStandardFunction("ABS", TypeNode.USInt, StandardLibraryFunction.IABS, TypeNode.USInt);
            InstallStandardFunction("ABS", TypeNode.UInt, StandardLibraryFunction.IABS, TypeNode.UInt);
            InstallStandardFunction("ABS", TypeNode.UDInt, StandardLibraryFunction.IABS, TypeNode.UDInt);
            InstallStandardFunction("ABS", TypeNode.ULInt, StandardLibraryFunction.LABS, TypeNode.ULInt);
            InstallStandardFunction("ABS", TypeNode.Real, StandardLibraryFunction.FABS, TypeNode.Real);
            InstallStandardFunction("ABS", TypeNode.LReal, StandardLibraryFunction.DABS, TypeNode.LReal);
            InstallStandardFunction("ABS", TypeNode.Time, StandardLibraryFunction.LABS, TypeNode.Time);
            InstallStandardFunction("TRUNC", TypeNode.AnyInt, StandardLibraryFunction.FTRUNC, TypeNode.Real);
            InstallStandardFunction("TRUNC", TypeNode.AnyInt, StandardLibraryFunction.DTRUNC, TypeNode.LReal);
            InstallStandardFunction("COS", TypeNode.Real, StandardLibraryFunction.FCOS, TypeNode.Real);
            InstallStandardFunction("COS", TypeNode.LReal, StandardLibraryFunction.DCOS, TypeNode.LReal);
            InstallStandardFunction("SIN", TypeNode.Real, StandardLibraryFunction.FSIN, TypeNode.Real);
            InstallStandardFunction("SIN", TypeNode.LReal, StandardLibraryFunction.DSIN, TypeNode.LReal);
            InstallStandardFunction("ACOS", TypeNode.Real, StandardLibraryFunction.FACOS, TypeNode.Real);
            InstallStandardFunction("ACOS", TypeNode.LReal, StandardLibraryFunction.DACOS, TypeNode.LReal);
            InstallStandardFunction("ASIN", TypeNode.Real, StandardLibraryFunction.FASIN, TypeNode.Real);
            InstallStandardFunction("ASIN", TypeNode.LReal, StandardLibraryFunction.DASIN, TypeNode.LReal);
            InstallStandardFunction("ATAN", TypeNode.Real, StandardLibraryFunction.FATAN, TypeNode.Real);
            InstallStandardFunction("ATAN", TypeNode.LReal, StandardLibraryFunction.DATAN, TypeNode.LReal);
            InstallStandardFunction("TAN", TypeNode.Real, StandardLibraryFunction.FTAN, TypeNode.Real);
            InstallStandardFunction("TAN", TypeNode.LReal, StandardLibraryFunction.DTAN, TypeNode.LReal);
            InstallStandardFunction("LN", TypeNode.Real, StandardLibraryFunction.FLOGN, TypeNode.Real);
            InstallStandardFunction("LN", TypeNode.LReal, StandardLibraryFunction.DLOGN, TypeNode.LReal);
            InstallStandardFunction("LOG", TypeNode.Real, StandardLibraryFunction.FLOG10, TypeNode.Real);
            InstallStandardFunction("LOG", TypeNode.LReal, StandardLibraryFunction.DLOG10, TypeNode.LReal);
            InstallStandardFunction("EXP", TypeNode.Real, StandardLibraryFunction.FEXP, TypeNode.Real);
            InstallStandardFunction("EXP", TypeNode.LReal, StandardLibraryFunction.DEXP, TypeNode.LReal);
            InstallStandardFunction("SQRT", TypeNode.Real, StandardLibraryFunction.FSQRT, TypeNode.Real);
            InstallStandardFunction("SQRT", TypeNode.LReal, StandardLibraryFunction.DSQRT, TypeNode.LReal);


            // Bit shift functions
            foreach (TypeNode intDataType in anySignedIntType)
            {
                foreach (TypeNode bitDataType in anyBitStringType)
                {
                    if (bitDataType.Size < TypeNode.LWord.Size)
                    {
                        InstallBitShiftFunction("SHL", bitDataType, VirtualMachineInstruction.IBSHL, bitDataType, intDataType);
                        InstallBitShiftFunction("SHR", bitDataType, VirtualMachineInstruction.IBSHR, bitDataType, intDataType);
                    }
                    else
                    {
                        InstallBitShiftFunction("SHL", bitDataType, VirtualMachineInstruction.LBSHL, bitDataType, intDataType);
                        InstallBitShiftFunction("SHR", bitDataType, VirtualMachineInstruction.LBSHR, bitDataType, intDataType);
                    }
                }
                InstallStandardFunction("ROR", TypeNode.Byte, VirtualMachineInstruction.BROR, TypeNode.Byte, intDataType);
                InstallStandardFunction("ROR", TypeNode.Word, VirtualMachineInstruction.WROR, TypeNode.Word, intDataType);
                InstallStandardFunction("ROR", TypeNode.DWord, VirtualMachineInstruction.DROR, TypeNode.DWord, intDataType);
                InstallStandardFunction("ROR", TypeNode.LWord, VirtualMachineInstruction.LROR, TypeNode.LWord, intDataType);
                InstallStandardFunction("ROL", TypeNode.Byte, VirtualMachineInstruction.BROL, TypeNode.Byte, intDataType);
                InstallStandardFunction("ROL", TypeNode.Word, VirtualMachineInstruction.WROL, TypeNode.Word, intDataType);
                InstallStandardFunction("ROL", TypeNode.DWord, VirtualMachineInstruction.DROL, TypeNode.DWord, intDataType);
                InstallStandardFunction("ROL", TypeNode.LWord, VirtualMachineInstruction.LROL, TypeNode.LWord, intDataType);  
            }

            // Character string functions
            InstallStandardFunction("LEN", TypeNode.Int, StandardLibraryFunction.SLEN, TypeNode.String);
            InstallStandardFunction("LEN", TypeNode.Int, StandardLibraryFunction.WSLEN, TypeNode.WString);
            foreach (TypeNode dataType in anyUnsignedIntType)
            {
                InstallStandardFunction("LEFT", TypeNode.String, StandardLibraryFunction.SLEFT, TypeNode.String, dataType);
                InstallStandardFunction("LEFT", TypeNode.WString, StandardLibraryFunction.WSLEFT, TypeNode.WString, dataType);
                InstallStandardFunction("RIGHT", TypeNode.String, StandardLibraryFunction.SRIGHT, TypeNode.String, dataType);
                InstallStandardFunction("RIGHT", TypeNode.WString, StandardLibraryFunction.WSRIGHT, TypeNode.WString, dataType);
                InstallStandardFunction("MID", TypeNode.String, StandardLibraryFunction.SMID, TypeNode.String, dataType);
                InstallStandardFunction("MID", TypeNode.WString, StandardLibraryFunction.WSMID, TypeNode.WString, dataType);
                InstallStandardFunction("INSERT", TypeNode.String, StandardLibraryFunction.SINSERT, TypeNode.String, TypeNode.String, dataType);
                InstallStandardFunction("INSERT", TypeNode.WString, StandardLibraryFunction.WSINSERT, TypeNode.WString, TypeNode.String, dataType);
                InstallStandardFunction("DELETE", TypeNode.String, StandardLibraryFunction.SDELETE, TypeNode.String, dataType, dataType);
                InstallStandardFunction("DELETE", TypeNode.WString, StandardLibraryFunction.WSDELETE, TypeNode.WString, dataType, dataType);
            }
            InstallStandardFunction("FIND", TypeNode.Int, StandardLibraryFunction.SFIND, TypeNode.String, TypeNode.String);
            InstallStandardFunction("FIND", TypeNode.Int, StandardLibraryFunction.WSFIND, TypeNode.WString, TypeNode.WString);

            TypeNode t1 = TypeNode.AnyInt;
            bool t = t1 == TypeNode.Byte;
            // Type conversion functions
            foreach (TypeNode fromDataType in allDataTypes)
            {
                foreach (TypeNode toDataType in allDataTypes)
                {
                    InstallTypeConvFunction(fromDataType, toDataType);
                }
            }
            // BCD_TO_ANYINT, ANYINT_TO_BCD conversions
            foreach (TypeNode anyBit in anyBitStringType)
            {
                foreach (TypeNode anyInt in anyIntType)
                {
                    InstallBCDConverterFunction(anyInt.Name + "_TO_BCD", anyBit, anyInt);
                    InstallBCDConverterFunction("BCD_TO_" + anyInt.Name, anyInt, anyBit);
                }
            }
            InstallTypeConverterFunction("DATE_AND_TIME_TO_TOD", TypeNode.TimeOfDay, TypeNode.DateAndTime);
            InstallTypeConverterFunction("DT_TO_TIME_OF_DAY", TypeNode.TimeOfDay, TypeNode.DateAndTime);
            InstallTypeConverterFunction("DT_TO_TOD", TypeNode.TimeOfDay,  TypeNode.DateAndTime);
            InstallTypeConverterFunction("TO_TOD", TypeNode.TimeOfDay,  TypeNode.DateAndTime);
            InstallTypeConverterFunction("DT_TO_DATE", TypeNode.Date, TypeNode.DateAndTime);
            InstallTypeConverterFunction("DT_TO_DT", TypeNode.DateAndTime,  TypeNode.DateAndTime);
            InstallTypeConverterFunction("TOD_TO_TOD", TypeNode.TimeOfDay,  TypeNode.TimeOfDay);

            InstallExtensibleFunction("MAX", TypeNode.SInt, StandardLibraryFunction.IMAX, TypeNode.SInt, TypeNode.SInt);
            InstallExtensibleFunction("MAX", TypeNode.Int, StandardLibraryFunction.IMAX, TypeNode.Int, TypeNode.Int);
            InstallExtensibleFunction("MAX", TypeNode.DInt, StandardLibraryFunction.IMAX, TypeNode.DInt, TypeNode.DInt);
            InstallExtensibleFunction("MAX", TypeNode.LInt, StandardLibraryFunction.LMAX, TypeNode.LInt, TypeNode.LInt);
            InstallExtensibleFunction("MAX", TypeNode.USInt, StandardLibraryFunction.IMAX, TypeNode.USInt, TypeNode.USInt);
            InstallExtensibleFunction("MAX", TypeNode.UInt, StandardLibraryFunction.IMAX, TypeNode.UInt, TypeNode.UInt);
            InstallExtensibleFunction("MAX", TypeNode.UDInt, StandardLibraryFunction.IMAX, TypeNode.UDInt, TypeNode.UDInt);
            InstallExtensibleFunction("MAX", TypeNode.ULInt, StandardLibraryFunction.LMAX, TypeNode.ULInt, TypeNode.ULInt);
            InstallExtensibleFunction("MAX", TypeNode.Byte, StandardLibraryFunction.IMAX, TypeNode.Byte, TypeNode.Byte);
            InstallExtensibleFunction("MAX", TypeNode.Word, StandardLibraryFunction.IMAX, TypeNode.Word, TypeNode.Word);
            InstallExtensibleFunction("MAX", TypeNode.DWord, StandardLibraryFunction.IMAX, TypeNode.DWord, TypeNode.DWord);
            InstallExtensibleFunction("MAX", TypeNode.LWord, StandardLibraryFunction.LMAX, TypeNode.LWord, TypeNode.LWord);
            InstallExtensibleFunction("MAX", TypeNode.Real, StandardLibraryFunction.FMAX, TypeNode.Real, TypeNode.Real);
            InstallExtensibleFunction("MAX", TypeNode.LReal, StandardLibraryFunction.DMAX, TypeNode.LReal, TypeNode.LReal);
            InstallExtensibleFunction("MAX", TypeNode.Date, StandardLibraryFunction.DTMAX, TypeNode.Date, TypeNode.Date);
            InstallExtensibleFunction("MAX", TypeNode.DateAndTime, StandardLibraryFunction.DTTMAX, TypeNode.DateAndTime, TypeNode.DateAndTime);
            InstallExtensibleFunction("MAX", TypeNode.Time, StandardLibraryFunction.TMAX, TypeNode.Time, TypeNode.Time);
            InstallExtensibleFunction("MAX", TypeNode.TimeOfDay, StandardLibraryFunction.TODMAX, TypeNode.TimeOfDay, TypeNode.TimeOfDay);
            InstallExtensibleFunction("MAX", TypeNode.String, StandardLibraryFunction.SMAX, TypeNode.String, TypeNode.String);
            InstallExtensibleFunction("MAX", TypeNode.WString, StandardLibraryFunction.WSMAX, TypeNode.WString, TypeNode.WString);

            InstallExtensibleFunction("MIN", TypeNode.SInt, StandardLibraryFunction.IMIN, TypeNode.SInt, TypeNode.SInt);
            InstallExtensibleFunction("MIN", TypeNode.Int, StandardLibraryFunction.IMIN, TypeNode.Int, TypeNode.Int);
            InstallExtensibleFunction("MIN", TypeNode.DInt, StandardLibraryFunction.IMIN, TypeNode.DInt, TypeNode.DInt);
            InstallExtensibleFunction("MIN", TypeNode.LInt, StandardLibraryFunction.LMIN, TypeNode.LInt, TypeNode.LInt);
            InstallExtensibleFunction("MIN", TypeNode.USInt, StandardLibraryFunction.IMIN, TypeNode.USInt, TypeNode.USInt);
            InstallExtensibleFunction("MIN", TypeNode.UInt, StandardLibraryFunction.IMIN, TypeNode.UInt, TypeNode.UInt);
            InstallExtensibleFunction("MIN", TypeNode.UDInt, StandardLibraryFunction.IMIN, TypeNode.UDInt, TypeNode.UDInt);
            InstallExtensibleFunction("MIN", TypeNode.ULInt, StandardLibraryFunction.LMIN, TypeNode.ULInt, TypeNode.ULInt);
            InstallExtensibleFunction("MIN", TypeNode.Byte, StandardLibraryFunction.IMIN, TypeNode.Byte, TypeNode.Byte);
            InstallExtensibleFunction("MIN", TypeNode.Word, StandardLibraryFunction.IMIN, TypeNode.Word, TypeNode.Word);
            InstallExtensibleFunction("MIN", TypeNode.DWord, StandardLibraryFunction.IMIN, TypeNode.DWord, TypeNode.DWord);
            InstallExtensibleFunction("MIN", TypeNode.LWord, StandardLibraryFunction.LMIN, TypeNode.LWord, TypeNode.LWord);
            InstallExtensibleFunction("MIN", TypeNode.Real, StandardLibraryFunction.FMIN, TypeNode.Real, TypeNode.Real);
            InstallExtensibleFunction("MIN", TypeNode.LReal, StandardLibraryFunction.DMIN, TypeNode.LReal, TypeNode.LReal);
            InstallExtensibleFunction("MIN", TypeNode.Date, StandardLibraryFunction.DTMIN, TypeNode.Date, TypeNode.Date);
            InstallExtensibleFunction("MIN", TypeNode.DateAndTime, StandardLibraryFunction.DTTMIN, TypeNode.DateAndTime, TypeNode.DateAndTime);
            InstallExtensibleFunction("MIN", TypeNode.Time, StandardLibraryFunction.TMIN, TypeNode.Time, TypeNode.Time);
            InstallExtensibleFunction("MIN", TypeNode.TimeOfDay, StandardLibraryFunction.TODMIN, TypeNode.TimeOfDay, TypeNode.TimeOfDay);
            InstallExtensibleFunction("MIN", TypeNode.String, StandardLibraryFunction.SMIN, TypeNode.String, TypeNode.String);
            InstallExtensibleFunction("MIN", TypeNode.WString, StandardLibraryFunction.WSMIN, TypeNode.WString, TypeNode.WString);

            InstallStandardFunction("LIMIT", TypeNode.SInt, StandardLibraryFunction.ILIMIT, TypeNode.SInt, TypeNode.SInt);
            InstallStandardFunction("LIMIT", TypeNode.Int, StandardLibraryFunction.ILIMIT, TypeNode.Int, TypeNode.Int);
            InstallStandardFunction("LIMIT", TypeNode.DInt, StandardLibraryFunction.ILIMIT, TypeNode.DInt, TypeNode.DInt);
            InstallStandardFunction("LIMIT", TypeNode.LInt, StandardLibraryFunction.LLIMIT, TypeNode.LInt, TypeNode.LInt);
            InstallStandardFunction("LIMIT", TypeNode.USInt, StandardLibraryFunction.ILIMIT, TypeNode.USInt, TypeNode.USInt);
            InstallStandardFunction("LIMIT", TypeNode.UInt, StandardLibraryFunction.ILIMIT, TypeNode.UInt, TypeNode.UInt);
            InstallStandardFunction("LIMIT", TypeNode.UDInt, StandardLibraryFunction.ILIMIT, TypeNode.UDInt, TypeNode.UDInt);
            InstallStandardFunction("LIMIT", TypeNode.ULInt, StandardLibraryFunction.LLIMIT, TypeNode.ULInt, TypeNode.ULInt);
            InstallStandardFunction("LIMIT", TypeNode.Byte, StandardLibraryFunction.ILIMIT, TypeNode.Byte, TypeNode.Byte);
            InstallStandardFunction("LIMIT", TypeNode.Word, StandardLibraryFunction.ILIMIT, TypeNode.Word, TypeNode.Word);
            InstallStandardFunction("LIMIT", TypeNode.DWord, StandardLibraryFunction.ILIMIT, TypeNode.DWord, TypeNode.DWord);
            InstallStandardFunction("LIMIT", TypeNode.LWord, StandardLibraryFunction.LLIMIT, TypeNode.LWord, TypeNode.LWord);
            InstallStandardFunction("LIMIT", TypeNode.Real, StandardLibraryFunction.FLIMIT, TypeNode.Real, TypeNode.Real);
            InstallStandardFunction("LIMIT", TypeNode.LReal, StandardLibraryFunction.DLIMIT, TypeNode.LReal, TypeNode.LReal);
            InstallStandardFunction("LIMIT", TypeNode.Date, StandardLibraryFunction.DTLIMIT, TypeNode.Date, TypeNode.Date);
            InstallStandardFunction("LIMIT", TypeNode.DateAndTime, StandardLibraryFunction.DTTLIMIT, TypeNode.DateAndTime, TypeNode.DateAndTime);
            InstallStandardFunction("LIMIT", TypeNode.Time, StandardLibraryFunction.TLIMIT, TypeNode.Time, TypeNode.Time);
            InstallStandardFunction("LIMIT", TypeNode.TimeOfDay, StandardLibraryFunction.TODLIMIT, TypeNode.TimeOfDay, TypeNode.TimeOfDay);
            InstallStandardFunction("LIMIT", TypeNode.String, StandardLibraryFunction.SLIMIT, TypeNode.String, TypeNode.String);
            InstallStandardFunction("LIMIT", TypeNode.WString, StandardLibraryFunction.WSLIMIT, TypeNode.WString, TypeNode.WString);

            // Selection functions
            foreach (TypeNode dataType in allDataTypes)
            {
                InstallStandardFunction("SEL", dataType, StandardLibraryFunction.SELECT, 1, TypeNode.Bool, dataType, dataType);
                
            }
            InstallExtensibleFunction("MUX", TypeNode.SInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.SInt, TypeNode.SInt);
            InstallExtensibleFunction("MUX", TypeNode.Int,  StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Int, TypeNode.Int);
            InstallExtensibleFunction("MUX", TypeNode.DInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.DInt, TypeNode.DInt);
            InstallExtensibleFunction("MUX", TypeNode.LInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.LInt, TypeNode.LInt);
            InstallExtensibleFunction("MUX", TypeNode.USInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.USInt, TypeNode.USInt);
            InstallExtensibleFunction("MUX", TypeNode.UInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.UInt, TypeNode.UInt);
            InstallExtensibleFunction("MUX", TypeNode.UDInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.UDInt, TypeNode.UDInt);
            InstallExtensibleFunction("MUX", TypeNode.ULInt, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.ULInt, TypeNode.ULInt);
            InstallExtensibleFunction("MUX", TypeNode.Byte, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Byte, TypeNode.Byte);
            InstallExtensibleFunction("MUX", TypeNode.Word, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Word, TypeNode.Word);
            InstallExtensibleFunction("MUX", TypeNode.DWord, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.DWord, TypeNode.DWord);
            InstallExtensibleFunction("MUX", TypeNode.LWord, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.LWord, TypeNode.LWord);
            InstallExtensibleFunction("MUX", TypeNode.Real, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Real, TypeNode.Real);
            InstallExtensibleFunction("MUX", TypeNode.LReal, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.LReal, TypeNode.LReal);
            InstallExtensibleFunction("MUX", TypeNode.Date, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Date, TypeNode.Date);
            InstallExtensibleFunction("MUX", TypeNode.DateAndTime, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.DateAndTime, TypeNode.DateAndTime);
            InstallExtensibleFunction("MUX", TypeNode.Time, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.Time, TypeNode.Time);
            InstallExtensibleFunction("MUX", TypeNode.TimeOfDay, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.TimeOfDay, TypeNode.TimeOfDay);
            InstallExtensibleFunction("MUX", TypeNode.String, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.String, TypeNode.String);
            InstallExtensibleFunction("MUX", TypeNode.WString, StandardLibraryFunction.MUX, TypeNode.AnyInt, TypeNode.WString, TypeNode.WString);

            // Standard function blocks
            FunctionBlockType fbDataType;
            List<InstanceSymbol> members;
            Expression initValue;

            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("S1", TypeNode.Bool, 0));
            members.Add(MakeInputVariable("R", TypeNode.Bool, 1));
            members.Add(MakeOutputVariable("Q1", TypeNode.Bool));
            fbDataType = MakeStdFunctionBlockType("SR", members);
            InstallStandardFunctionBlock("SR", fbDataType, members, 0);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("S", TypeNode.Bool, 0));
            members.Add(MakeInputVariable("R1", TypeNode.Bool, 1));
            members.Add(MakeOutputVariable("Q1", TypeNode.Bool));
            fbDataType = MakeStdFunctionBlockType("RS", members);
            InstallStandardFunctionBlock("RS", fbDataType, members, 1);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("CLK", TypeNode.Bool, 0));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            initValue = new BoolConstant(false);
            members.Add(MakeLocalVariable("MEM", TypeNode.Bool, STVarQualifier.RETAIN, initValue, 1));
            fbDataType = MakeStdFunctionBlockType("R_TRIG", members);
            InstallStandardFunctionBlock("R_TRIG", fbDataType, members, 2);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("CLK", TypeNode.Bool, 0));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            initValue = new BoolConstant(true);
            members.Add(MakeLocalVariable("MEM", TypeNode.Bool, STVarQualifier.RETAIN, initValue, 1));
            fbDataType = MakeStdFunctionBlockType("F_TRIG", members);
            InstallStandardFunctionBlock("F_TRIG", fbDataType, members, 3);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("CU", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("R", TypeNode.Bool, 1));
            members.Add(MakeInputVariable("PV", TypeNode.Bool, 2));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("CV", TypeNode.Int));
            fbDataType = MakeStdFunctionBlockType("CTU", members);
            InstallStandardFunctionBlock("CTU", fbDataType, members, 4);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("CD", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("LD", TypeNode.Int, 1));
            members.Add(MakeInputVariable("PV", TypeNode.Int, 2));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("CV", TypeNode.Int));
            fbDataType = MakeStdFunctionBlockType("CTD", members);
            InstallStandardFunctionBlock("CTD", fbDataType, members, 5);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("CU", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("CD", TypeNode.Bool, 1, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("R", TypeNode.Bool, 2));
            members.Add(MakeInputVariable("LD", TypeNode.Bool, 3));
            members.Add(MakeInputVariable("PV", TypeNode.Int, 4));
            members.Add(MakeOutputVariable("QU", TypeNode.Bool));
            members.Add(MakeOutputVariable("QD", TypeNode.Bool));
            members.Add(MakeOutputVariable("CV", TypeNode.Int));
            fbDataType = MakeStdFunctionBlockType("CTUD", members);
            InstallStandardFunctionBlock("CTUD", fbDataType, members, 6);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("IN", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("PT", TypeNode.Time, 1, STDeclQualifier.R_EDGE));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("ET", TypeNode.Time));
            fbDataType = MakeStdFunctionBlockType("TP", members);
            InstallStandardFunctionBlock("TP", fbDataType, members, 7);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("IN", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("PT", TypeNode.Time, 1, STDeclQualifier.R_EDGE));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("ET", TypeNode.Time));
            fbDataType = MakeStdFunctionBlockType("TON", members);
            InstallStandardFunctionBlock("TON", fbDataType, members, 8);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("IN", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("PT", TypeNode.Time, 1, STDeclQualifier.R_EDGE));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("ET", TypeNode.Time));
            fbDataType = MakeStdFunctionBlockType("TOF", members);
            InstallStandardFunctionBlock("TOF", fbDataType, members, 9);
            members = new List<InstanceSymbol>();
            members.Add(MakeInputVariable("EN", TypeNode.Bool, 0, STDeclQualifier.R_EDGE));
            members.Add(MakeInputVariable("PDT", TypeNode.DateAndTime, 1, STDeclQualifier.R_EDGE));
            members.Add(MakeOutputVariable("Q", TypeNode.Bool));
            members.Add(MakeOutputVariable("CDT", TypeNode.DateAndTime));
            fbDataType = MakeStdFunctionBlockType("RTC", members);
            InstallStandardFunctionBlock("RTC", fbDataType, members, 10);
        }

        private const int SYSTEM_LEVEL = 0;

        private const int USER_LEVEL = 1;

        private const int LOCAL_LEVEL = 2;

        private const int MAX_LEXICAL_SCOPES = 3;

        private int scopeLevel;

        private readonly ErrorHandler report;

        private UserDefinedFunctionSymbol currentFunction;

        private UserDefinedFunctionBlockSymbol currentFunctionBlock;

        private readonly List<ProgramOrganizationUnitSymbol> userDefinedPOUList;

        private static readonly Dictionary<string, STLangSymbol>[] symbolDictionary;
    }
}
