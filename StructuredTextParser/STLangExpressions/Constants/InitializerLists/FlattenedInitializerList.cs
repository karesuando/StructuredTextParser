using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.DataTypes;
using STLang.Expressions;
using STLang.ErrorManager;
using STLang.Extensions;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class FlattenedInitializerList : InitializerList
    {

        public FlattenedInitializerList(TypeNode dataType, Expression size) 
            : base(dataType, size)
        {
            this.initializerList = new List<Expression>();
        }

        public override void Add(Expression initializer, LexLocation loc = null)
        {
            if (initializer == null)
                return;
            else if (initializer is ArrayInitializer)
            {
                ArrayInitializer arrayInit;
                IEnumerable<Expression> flattenedList;

                arrayInit = (ArrayInitializer)initializer;
                flattenedList = arrayInit.FlattenedInitializerList;
                foreach (Expression initialValue in flattenedList)
                {
                    if (this.initializerString.Length > 0)
                        this.initializerString += ",";
                    this.initializerString += initialValue.ToString();
                }
                this.initializerList.AddRange(flattenedList);
                if (!arrayInit.IsConstant)
                    this.isConstant = false;
                if (!arrayInit.IsZero)
                    this.isZero = false;
            }
            else if (initializer is InitializerSequence)
            {
                InitializerSequence initSequence;
                initSequence = (InitializerSequence)initializer;
                foreach (Expression initialValue in initSequence.ElementList)
                    this.Add(initialValue);
            }
            else if (initializer is FlattenedInitializerList)
            {
                FlattenedInitializerList flattenedList;

                flattenedList = (FlattenedInitializerList)initializer;
                if (flattenedList.Count > 0)
                {
                    IEnumerable<Expression> initializers;
                    initializers = flattenedList.InitializerList;
                    foreach (Expression initialValue in initializers)
                    {
                        if (this.initializerString.Length > 0)
                            this.initializerString += ",";
                        this.initializerString += initialValue.ToString();
                    }
                    this.initializerList.AddRange(initializers);
                    if (!flattenedList.IsConstant)
                        this.isConstant = false;
                    if (!flattenedList.IsZero)
                        this.isZero = false;
                }
            }
            else if (initializer is Expression)
            {
                this.initializerList.Add(initializer);
                if (this.initializerString.Length > 0)
                    this.initializerString += ",";
                this.initializerString += initializer.ToString();
                if (!initializer.IsConstant)
                    this.isConstant = false;
                if (!initializer.IsZero)
                    this.isZero = false;
            }
            else
            {
                string msg = "FlattenedInitializerList.Add() failed.";
                msg += " Illegal type of array initializer: " + initializer;
                throw new STLangCompilerError(msg);
            }
        }

        public List<T> ConvertTo<T>(ListExtension.Converter<T> converter)
        {
            return this.initializerList.ConvertTo(converter);
        }

        public IEnumerable<Expression> InitializerList
        {
            get { return this.initializerList; }
        }

        public override int Count
        {
            get { return this.initializerList.Count; }
        }

        public override string ToString()
        {
            return "[" + this.initializerString + "]";
        }

        private readonly List<Expression> initializerList;
    }
}
