using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Symbols;
using STLang.DataTypes;
using STLang.ErrorManager;
using QUT.Gppg;

namespace STLang.Expressions
{
    public class DefaultArrayOfStructInitializer : ArrayOfStructInitializer
    {
        public DefaultArrayOfStructInitializer(TypeNode dataType, Expression size, 
                                        Dictionary<string, InitializerList> flattenedInitLists)
            : base(dataType, size, flattenedInitLists)
        {
            Expression defaultValue = this.elementType.DefaultValue;
            for (int i = this.Count; i > 0; i--)
                this.initializerList.Add(defaultValue);
            this.initializerString = this.Count + "(" + defaultValue + ")";
            foreach (InitializerList initList in this.FlattenedInitializerLists)
            {

            }
        }

        public override void Add(Expression initValue, LexLocation location)
        {
            string msg = "DefaultArrayOfStructInitializer.Add() not implemented.";
            throw new NotImplementedException(msg);
        }

        public override void AddInitializer(string fieldName, InitializerList initializer)
        {
            string msg = "DefaultArrayOfStructInitializer.AddInitializer() not implemented.";
            throw new NotImplementedException(msg);
        }

        public override void GenerateLoad(List<int> trueBranch = null, List<int> falseBranch = null)
        {
            string key;
            FieldSymbol field = this.structure.FirstField;
            InitializerList flattenedInitList;

            while (field != null)
            {
                key = field.Name.ToUpper();
                flattenedInitList = this.flattenedInitLists[key];
                flattenedInitList.GenerateLoad();
                field = field.Next;
            }
        }
    }
}
