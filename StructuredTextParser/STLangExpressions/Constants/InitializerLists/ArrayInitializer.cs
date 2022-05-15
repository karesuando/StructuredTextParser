using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.Extensions;
using STLang.ErrorManager;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class ArrayInitializer : InitializerList
    {
        public ArrayInitializer(TypeNode dataType, Expression size)
            : base(dataType, size)
        {
            this.initializerList = new List<Expression>();
            this.flattenedInitList = new FlattenedInitializerList(dataType, size);
            initListOverflow = false;
            if (dataType.IsArrayType)
            {
                ArrayType array = (ArrayType)dataType;
                this.basicElementType = array.BasicElementType;
                this.elementCount = (int)(array.Size / this.basicElementType.Size);
            }
            else
            {
                this.basicElementType = TypeNode.Error;
                this.elementCount = 0;
            }
        }

        public ArrayInitializer(List<Expression> flattenedInitList, TypeNode dataType, Expression size) 
            : this(dataType, size)
        {
            this.flattenedInitList = new FlattenedInitializerList(dataType, size);
            foreach (Expression initValue in flattenedInitList)
            {
                if (!initValue.IsConstant)
                    this.isConstant = false;
                if (!initValue.IsZero)
                    this.isZero = false;
                this.flattenedInitList.Add(initValue);
            }
            initListOverflow = false;
        }

        public ArrayInitializer CreateInitList(List<Expression> flattenedList)
        {
            return new ArrayInitializer(flattenedList, this.DataType, this.Size);
        }

        public override void Add(Expression initializer, LexLocation location)
        {
            if (initializer == null)
                return;
            else if (this.DataType == TypeNode.Error)
                return;
            else if (initializer is InitializerSequence)
            {
                InitializerSequence initializerSeq;
                initializerSeq = (InitializerSequence)initializer;
                int elementSum = this.initializerList.Count + initializerSeq.Count;
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initializerSeq.ToString();
                if (elementSum <= this.elementCount)
                {
                    IEnumerable<Expression> initSequence;
                    initSequence = initializerSeq.ElementList;
                    this.initializerList.AddRange(initSequence);
                    this.flattenedInitList.Add(initializerSeq);
                    foreach (Expression initialValue in initSequence)
                    {
                        if (!initialValue.IsConstant)
                            this.isConstant = false;
                        if (!initialValue.IsZero)
                            this.isZero = false;
                    }
                }
                else if (this.initializerList.Count < this.elementCount)
                {
                    // Too many initializers
                    int count = this.elementCount - this.initializerList.Count;
                    IEnumerable<Expression> initializers;
                    initializers = initializerSeq.ElementList.Take(count);
                    foreach (Expression element in initializers)
                    {
                        this.initializerList.Add(element);
                        this.flattenedInitList.Add(element);
                        if (!element.IsConstant)
                            this.isConstant = false;
                        if (!element.IsZero)
                            this.isZero = false;
                    }
                    Report.Warning(21, location);
                }
            }
            else if (this.initializerList.Count >= this.elementCount)
            {
                if (!this.initListOverflow)
                {
                    this.initListOverflow = true;
                    Report.Warning(21, location);
                }
            }
            else
            {
                this.initializerList.Add(initializer);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initializer.ToString();
                this.flattenedInitList.Add(initializer);
                if (!initializer.IsConstant)
                    this.isConstant = false;
                if (!initializer.IsZero)
                    this.isZero = false;
            }
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            //throw new NotImplementedException("ArrayInitializer.GenerateLoad() not implemented.");
        }

        public override InitializerList Expand(int factor, ErrorHandler report)
        {
            InitializerSequence initSequence = new InitializerSequence(this.DataType);
            for (int i = 0; i < factor; i++)
            {
                initSequence.Add(this.flattenedInitList, null);
            }
            return initSequence;
        }

        public override void CheckInitListSize(LexLocation location)
        {
            if (this.initializerList.Count < this.elementCount)
                Report.Warning(20, location);
        }

        public IEnumerable<Expression> InitializerList
        {
            get { return this.initializerList; }
        }

        public IEnumerable<Expression> FlattenedInitializerList
        {
            get { return this.flattenedInitList.InitializerList; }
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

        public override byte[] GetBytes()
        {
            if (this.basicElementType == TypeNode.LReal)
                return this.flattenedInitList.ConvertTo(Convert.ToDouble).GetBytes();
            else if (this.basicElementType == TypeNode.Real)
                return this.flattenedInitList.ConvertTo(Convert.ToSingle).GetBytes();
            else if (this.basicElementType == TypeNode.LInt)
                return this.flattenedInitList.ConvertTo(Convert.ToInt64).GetBytes();
            else if (this.basicElementType == TypeNode.DInt)
                return this.flattenedInitList.ConvertTo(Convert.ToInt32).GetBytes();
            else if (this.basicElementType == TypeNode.Int)
                return this.flattenedInitList.ConvertTo(Convert.ToInt16).GetBytes();
            else if (this.basicElementType == TypeNode.SInt)
                return this.flattenedInitList.ConvertTo(Convert.ToSByte).GetBytes();
            else if (this.basicElementType == TypeNode.ULInt)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt64).GetBytes();
            else if (this.basicElementType == TypeNode.UDInt)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt32).GetBytes();
            else if (this.basicElementType == TypeNode.UInt)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt16).GetBytes();
            else if (this.basicElementType == TypeNode.USInt)
                return this.flattenedInitList.ConvertTo(Convert.ToByte).GetBytes();
            else if (this.basicElementType == TypeNode.LWord)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt64).GetBytes();
            else if (this.basicElementType == TypeNode.DWord)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt32).GetBytes();
            else if (this.basicElementType == TypeNode.Word)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt16).GetBytes();
            else if (this.basicElementType == TypeNode.Byte)
                return this.flattenedInitList.ConvertTo(Convert.ToByte).GetBytes();
            else if (this.basicElementType == TypeNode.Bool)
                return this.flattenedInitList.ConvertTo(Convert.ToByte).GetBytes();
            else if (this.basicElementType == TypeNode.Date)
                return this.flattenedInitList.ConvertTo(Convert.ToDateTime).GetBytes();
            else if (this.basicElementType == TypeNode.DateAndTime)
                return this.flattenedInitList.ConvertTo(Convert.ToDateTime).GetBytes();
            else if (this.basicElementType == TypeNode.Time)
                return this.flattenedInitList.ConvertTo(t => (TimeSpan)t).GetBytes();
            else if (this.basicElementType == TypeNode.TimeOfDay)
                return this.flattenedInitList.ConvertTo(t => (TimeSpan)t).GetBytes();
            else if (this.basicElementType.IsEnumeratedType)
                return this.flattenedInitList.ConvertTo(Convert.ToUInt16).GetBytes();
            else if (this.basicElementType.IsStringType)
            {
                int length = (int)this.basicElementType.Size;
                return this.flattenedInitList.ConvertTo(Convert.ToString).GetAscIIBytes(length);
            }
            else if (this.basicElementType.IsWStringType)
            {
                int length = (int)this.basicElementType.Size;
                return this.flattenedInitList.ConvertTo(Convert.ToString).GetUnicodeBytes(length);
            }
            else
            {
                string msg = "ArrayInitializer.GetBytes() failed for data type ";
                throw new STLangCompilerError(msg + this.DataType.Name);
            }
        }

        public override string ToString()
        {
            return "[" + this.initializerString + "]";
        }

        public Expression GetElementAt(params int[] index)
        {
            int i = index[0];
            if (i < 0 || i >= this.initializerList.Count)
                i = 0;
            Expression arrayElement = this.initializerList[i];
            if (index.Length == 1)
                return arrayElement;
            else if (arrayElement is ArrayInitializer)
            {
                ArrayInitializer arrayInit = (ArrayInitializer)arrayElement;
                index = index.Skip(1).ToArray();
                return arrayInit.GetElementAt(index);
            }
            throw new STLangCompilerError(index + ": Too many array indexes.");
        }

        public TypeNode BasicElementType
        {
            get { return this.basicElementType; }
        }


        private bool initListOverflow;

        protected readonly int elementCount;

        protected readonly TypeNode basicElementType;

        protected readonly List<Expression> initializerList;

        protected readonly FlattenedInitializerList flattenedInitList;
    }
}
