using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using QUT.Gppg;
using STLang.Symbols;
using STLang.ErrorManager;
using STLang.MemoryLayout;
using System.Collections;

namespace STLang.Expressions
{
    public class ArrayOfStructInitializer : InitializerList
    {
        public ArrayOfStructInitializer(TypeNode dataType, Expression size)
            : base(dataType, size)
        {
            if (!dataType.IsArrayType)
                throw new STLangCompilerError("Array initializer type must be an array.");
            else {
                ArrayType array = (ArrayType)dataType.BaseType;
                if (! array.BasicElementType.IsStructType)
                    throw new STLangCompilerError("Array element type must be a structure.");
                else
                {
                    this.structure = (StructType)array.BasicElementType.BaseType;
                    int fieldCount = this.structure.FieldCount;
                    this.initializerList = new List<Expression>();
                    this.flattenedInitLists = new Dictionary<string, InitializerList>();
                    this.elementCount = array.Range;
                    this.elementType = array.ElementType;
                    this.initListOverflow = false;
                    this.appendToInitString = true;
                }
            }
        }

        public ArrayOfStructInitializer(TypeNode dataType, Expression size, Dictionary<string, InitializerList> initList)
            : base(dataType, size)
        {
            if (!dataType.IsArrayType)
                throw new STLangCompilerError("Array initializer type must be an array.");
            else
            {
                ArrayType array = (ArrayType)dataType.BaseType;
                if (!array.BasicElementType.IsStructType)
                    throw new STLangCompilerError("Array element type must be a structure.");
                else
                {
                    this.structure = (StructType)array.BasicElementType.BaseType;
                    int fieldCount = this.structure.FieldCount;
                    this.initializerList = new List<Expression>();
                    this.flattenedInitLists = initList;
                    this.elementCount = array.Range;
                    this.elementType = array.ElementType;
                    this.initListOverflow = false;
                    this.appendToInitString = true;
                }
            }
        }

        public virtual void AddInitializer(string fieldName, InitializerList initializer)
        {
            string key = fieldName.ToUpper();
            this.flattenedInitLists.Add(key, initializer);
        }

        public override void Add(Expression initValue, LexLocation location)
        {
            if (initValue == null)
                return;
            else if (initValue is InitializerSequence)
            {
                InitializerSequence initSequence;
                initSequence = (InitializerSequence)initValue;
                this.appendToInitString = false;
                foreach (Expression initializer in initSequence.ElementList)
                    this.Add(initializer, location);
                this.appendToInitString = true;
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initSequence.ToString();
            }
            else if (this.initializerList.Count >= this.elementCount)
            {
                if (!this.initListOverflow)
                {
                    this.initListOverflow = true;
                    Report.Warning(21, location);
                }
            }
            else if (initValue is StructInitializer)
            {
                string key;
                FieldSymbol field;
                Expression fieldInitializer;
                StructInitializer structInit = (StructInitializer)initValue;

                this.initializerList.Add((Expression)initValue);
                if (!structInit.IsConstant)
                    this.isConstant = false;
                if (!structInit.IsZero)
                    this.isZero = false;
                field = structure.FirstField;
                while (field != null)
                {
                    key = field.Name.ToUpper();
                    if (!structInit.Contains(field.Name, out fieldInitializer))
                    {
                        string msg;
                        msg = string.Format("Field {0} not found in struct initializer.", field.Name);
                        throw new STLangCompilerError(msg);
                    }
                    else if (this.flattenedInitLists.ContainsKey(key))
                    {
                        InitializerList flattenedList;

                        flattenedList = this.flattenedInitLists[key];
                        flattenedList.Add(fieldInitializer, location);
                    }
                    if (!fieldInitializer.IsConstant)
                        this.isConstant = false;
                    field = field.Next;
                }
                if (this.appendToInitString)
                {
                    if (this.initializerString.Length > 0)
                        this.initializerString += ",";
                    this.initializerString += initValue.ToString();
                }
            }
            else if (initValue is ArrayOfStructInitializer
                  || initValue is DefaultArrayOfStructInitializer)
            {
                FieldSymbol field;
                ArrayOfStructInitializer arrayOfStructInit;
                IEnumerable<InitializerList> flattenedInitLists;

                field = this.structure.FirstField;
                this.initializerList.Add((Expression)initValue);
                arrayOfStructInit = (ArrayOfStructInitializer)initValue;
                flattenedInitLists = arrayOfStructInit.FlattenedInitializerLists;
                if (!arrayOfStructInit.IsConstant)
                    this.isConstant = false;
                if (!arrayOfStructInit.IsZero)
                    this.isZero = false;
                foreach (InitializerList initList in flattenedInitLists)
                {
                    if (field == null)
                    {
                        string msg = "ArrayOfStructInitalizer.Add(): Field is null";
                        throw new STLangCompilerError(msg);
                    }
                    else {
                        string key = field.Name.ToUpper();
                        if (!this.flattenedInitLists.ContainsKey(key))
                        {
                            string msg = "ArrayOfStructInitalizer.Add(): ";
                            msg += "Field " + field.Name + " not found.";
                            throw new STLangCompilerError(msg);
                        }
                        else
                        {
                            InitializerList flattenedList;

                            flattenedList = this.flattenedInitLists[key];
                            flattenedList.Add(initList, location);
                        }
                        field = field.Next;
                    }
                }
                if (this.appendToInitString)
                {
                    if (this.initializerString.Length > 0)
                        this.initializerString += ",";
                    this.initializerString += initValue.ToString();
                }
            }
            else
            {
                string msg = initValue.ToString();
                msg += ": Array initializer element must be a struct initializer.";
                throw new STLangCompilerError(msg);
            }
        }

        public override string ToString()
        {
            return "[" + this.initializerString + "]";
        }

        public IEnumerable<Expression> InitializerList
        {
            get { return this.initializerList; }
        }

        public override string GetKey()
        {
            if (!this.DataType.IsArrayType)
                return "";
            else if (this.initializerList == null || this.initializerList.Count == 0)
                return this.DataType.TypeID + "[]";
            else
            {
                string stringValue = this.initializerList[0].GetKey();
                foreach (Expression initializer in this.initializerList.Skip(1))
                {
                    stringValue += "," + initializer.GetKey();
                }
                return this.DataType.TypeID + "[" + stringValue + "]";
            }
        }

        public override void CheckInitListSize(LexLocation location)
        {
            if (this.initializerList.Count < this.elementCount)
                Report.Warning(20, location);
        }

        public TypeNode BasicElementType
        {
            get { return this.structure; }
        }

        public override int Count
        {
            get { return this.elementCount; }
        }

        public InitializerList GetFlattenedInitializerList(string field)
        {
            string key = field.ToUpper();
            if (this.flattenedInitLists.ContainsKey(key))
                return this.flattenedInitLists[key];
            else
            {
                string msg;
                msg = "GetFlattenedInitializerList(): " + field + " not found.";
                throw new STLangCompilerError(msg);
            }
        }

        public IEnumerable<InitializerList> FlattenedInitializerLists
        {
            get 
            {
                string key;
                FieldSymbol field = this.structure.FirstField;
                
                while (field != null)
                {
                    key = field.Name.ToUpper();
                    if (this.flattenedInitLists.ContainsKey(key))
                        yield return this.flattenedInitLists[key];
                    else
                    {
                        string msg;
                        msg = "getter FlattenedInitializerList: Field " + field + " not found.";
                        throw new STLangCompilerError(msg);
                    }
                    field = field.Next;
                }
            }
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            string key;
            FieldSymbol field = this.structure.FirstField;

            while (field != null)
            {
                key = field.Name.ToUpper();
                if (!this.flattenedInitLists.ContainsKey(key))
                {
                    string msg = "ArrayOfStructInitializer.GenerateLoad() failed.";
                    throw new STLangCompilerError(msg);
                }
                else
                {
                    InitializerList initializer;

                    initializer = (InitializerList)this.flattenedInitLists[key];
                    initializer.GenerateLoad();
                }
                field = field.Next;
            }
        }

        public Expression GetElementAt(params int[] index)
        {
            int i = index[0];
            if (i < 0 || i >= this.initializerList.Count)
                return Expression.Error;
            else
            {
                Expression arrayElement = this.initializerList[i];
                if (index.Length == 1)
                    return arrayElement;
                else if (arrayElement is ArrayOfStructInitializer)
                {
                    ArrayOfStructInitializer arrayInit;
                    arrayInit = (ArrayOfStructInitializer)arrayElement;
                    index = index.Skip(1).ToArray();
                    return arrayInit.GetElementAt(index);
                }
                throw new STLangCompilerError(index + ": Too many array indexes.");
            }
        }

        private bool initListOverflow;

        private bool appendToInitString;

        private readonly int elementCount;

        protected readonly TypeNode elementType;

        protected readonly StructType structure;

        protected readonly List<Expression> initializerList;

        protected readonly Dictionary<string, InitializerList> flattenedInitLists;
    }
}
