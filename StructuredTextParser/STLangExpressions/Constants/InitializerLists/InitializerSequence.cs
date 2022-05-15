using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.ErrorManager;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class InitializerSequence : InitializerList
    {
        public InitializerSequence(TypeNode elementDataType) 
            : base(elementDataType, Expression.Error)
        {
            this.elementList = new List<Expression>();
        }

        private InitializerSequence(TypeNode elementDataType, string initializerStr)
            : base(elementDataType, Expression.Error)
        {
            this.elementList = new List<Expression>();
            this.initializerString = initializerStr;
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            throw new STLangCompilerError("InitializerSequence.GenerateCode() not implemented.");
        }

        public override InitializerList Expand(int factor, ErrorHandler report)
        {
            InitializerSequence initSequence;
            string initString = factor + "(" + this.initializerString + ")";
            initSequence = new InitializerSequence(this.DataType, initString);
            for (int i = 0; i < factor; i++)
                initSequence.AddRange(this.elementList);
            return initSequence;
        }

        public override void CheckInitListSize(LexLocation location)
        {
        }

        public override void Add(Expression initElem, LexLocation loc)
        {
            if (initElem == null)
                return;
            else if (initElem is InitializerSequence)
            {
                InitializerSequence initSequence = (InitializerSequence)initElem;
                this.elementList.AddRange(initSequence.ElementList);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initSequence.ToString();
                if (!initSequence.IsConstant)
                    this.isConstant = false;
                if (!initSequence.IsZero)
                    this.isZero = false;
            }
            else if (initElem is FlattenedInitializerList)
            {
                FlattenedInitializerList flattenedList;

                flattenedList = (FlattenedInitializerList)initElem;
                this.elementList.AddRange(flattenedList.InitializerList);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += flattenedList.ToString();
                if (!flattenedList.IsConstant)
                    this.isConstant = false;
                if (!flattenedList.IsZero)
                    this.isZero = false;
            }
            else if (initElem is ArrayOfStructInitializer)
            {
                ArrayOfStructInitializer arrayOfStructs;

                arrayOfStructs = (ArrayOfStructInitializer)initElem;
                this.elementList.AddRange(arrayOfStructs.InitializerList);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += arrayOfStructs.ToString();
                if (!arrayOfStructs.IsConstant)
                    this.isConstant = false;
                if (!arrayOfStructs.IsZero)
                    this.isZero = false;
            }
            else if (initElem is Expression)
            {
                Expression initializer = (Expression)initElem;
                this.elementList.Add(initializer);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initializer.ToString();
                if (!initializer.IsConstant)
                    this.isConstant = false;
                if (!initializer.IsZero)
                    this.isZero = false;
            }
        }

        public override string ToString()
        {
            return this.initializerString;
        }

        private void AddRange(List<Expression> initList)
        {
            this.elementList.AddRange(initList);
        }

        public override int Count
        {
            get { return this.elementList.Count; }
        }

        public IEnumerable<Expression> ElementList 
        { 
            get { return this.elementList; } 
        }

        private readonly List<Expression> elementList;
    }
}
