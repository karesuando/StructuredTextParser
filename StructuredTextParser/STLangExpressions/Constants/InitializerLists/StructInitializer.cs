using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.ErrorManager;
using STLang.ParserUtility;

using QUT.Gppg;

namespace STLang.Expressions
{
    public class StructInitializer : InitializerList
    {
        public StructInitializer(TypeNode dataType, Expression size)
            : base(dataType, size)
        {
            this.initializers = new Dictionary<string, Expression>();
            if (!dataType.IsStructType)
            {
                this.fieldCount = 0;
                this.structure = null;
            }
            else
            {
                this.structure = (StructType)dataType.BaseType;
                this.fieldCount = structure.FieldCount;
            }
        }

        public override void Add(Expression initializer, LexLocation location)
        {
            if (this.DataType == TypeNode.Error)
                return; // Error struct type
            else if (initializer == null)
                return;
            else if (!(initializer is StructMemberInit))
                Report.SyntaxError(10, location);
            else {
                StructMemberInit member;
                member = (StructMemberInit)initializer;
                string fieldName = member.Member;
                string key = fieldName.ToUpper();
                Expression initialValue = member.InitValue;
                if (this.initializers.ContainsKey(key))
                    Report.Warning(23, fieldName, location);
                else
                    this.initializers[key] = member.InitValue;
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += member.Member + " := " + initialValue;
                if (initialValue.IsConstant)
                    this.isZero = initialValue.IsZero;
                else {
                    this.isConstant = false;
                    this.isZero = false;
                }
            }
        }

        public override InitializerList Expand(int factor, ErrorHandler report)
        {
            throw new STLangCompilerError("Cannot expand structure initializer list");
        }

        public override void CheckInitListSize(LexLocation location)
        {
            if (!this.DataType.IsStructType)
                return;
            else if (this.initializers.Count < this.fieldCount)
            {
                string key;
                FieldSymbol field = this.structure.FirstField;

                while (field != null)
                {
                    key = field.Name.ToUpper();
                    if (!this.initializers.ContainsKey(key))
                    {
                        Expression initialValue = field.InitialValue;
                        this.initializers[key] = initialValue;
                        if (initialValue.IsConstant)
                            this.isZero = initialValue.IsZero;
                        else
                        {
                            this.isConstant = false;
                            this.isZero = false;
                        }
                        Report.Warning(22, field.Name, initialValue, location);
                    }
                    field = field.Next;
                }
            }
        }

        public Expression GetFieldInitializer(string fieldName)
        {
            if (!this.DataType.IsStructType)
                return Expression.Error;
            else
            {
                FieldSymbol fieldSymbol;
                string key = fieldName.ToUpper();
                if (!this.structure.LookUp(fieldName, out fieldSymbol))
                {
                    string msg;
                    msg = "GetFieldInitializer(): Field " + fieldName + " not found in struct.";
                    throw new STLangCompilerError(msg);
                }
                else if (this.initializers.ContainsKey(key))
                    return this.initializers[key];
                else
                    return fieldSymbol.DataType.DefaultValue;
            }
        }

        public override int Count
        {
            get { return this.fieldCount; }
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            string key;
            Expression initialValue;
            FieldSymbol fieldSymbol = this.structure.FirstField;

            while (fieldSymbol != null)
            {
                key = fieldSymbol.Name.ToUpper();
                if (!this.initializers.ContainsKey(key))
                {
                    string msg = "StructInitializer.GenerateCode(): Field "
                               + fieldSymbol.Name + " not found in struct.";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    initialValue = this.initializers[key];
                    initialValue.GenerateLoad();
                }
                fieldSymbol = fieldSymbol.Next;
            }
        }

        public bool Contains(string field, out Expression initializer)
        {
            string key = field.ToUpper();
            if (!this.initializers.ContainsKey(key))
            {
                initializer = Expression.Error;
                return false;
            }
            else
            {
                initializer = this.initializers[key];
                return true;
            }
        }

        public override string GetKey()
        {
            if (!this.DataType.IsStructType)
                return "";
            else
            {
                string key;
                Expression initialValue;
                string initializerList = string.Empty;
                FieldSymbol fieldSym = this.structure.FirstField;

                while (fieldSym != null)
                {
                    key = fieldSym.Name.ToUpper();
                    if (!this.initializers.ContainsKey(key))
                        throw new STLangCompilerError("GetKey(): Field " + fieldSym.Name
                                                    + " not found in struct.");
                    else
                    {
                        initialValue = this.initializers[key];
                        if (initializerList.Length > 0)
                            initializerList += ",";
                        initializerList += initialValue.GetKey();
                    }
                    fieldSym = fieldSym.Next;
                }
                return this.structure.TypeID + "(" + initializerList + ")";
            }
        }

        public override string ToString()
        {
            return "(" + this.initializerString + ")";
        }

        protected readonly int fieldCount;

        protected readonly StructType structure;

        protected readonly Dictionary<string, Expression> initializers;
    }
}
