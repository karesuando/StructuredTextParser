using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;
using STLang.Symbols;
using System.Collections;

namespace STLang.DataTypes
{
    public class StructType : TypeNode
    {
        public StructType()
            : base("STRUCT()", 0, "()")
        {
            Expression size = new IntConstant(0);
            this.firstField = null;
            this.fieldCount = 0;
            this.isStoredContiguously = false;
            this.fieldTable = new Dictionary<string, FieldSymbol>();
            this.initialValue = new DefaultStructInitializer(this, size);
        }

        public StructType(Dictionary<string, FieldSymbol> members, FieldSymbol firstField,
                          uint byteCount, Expression size, bool isStoredContig, string typeName, string typeID)
            : base(typeName, byteCount, typeID)
        {
            this.fieldTable = members;
            this.firstField = firstField;
            this.isStoredContiguously = isStoredContig;
            this.fieldCount = members.Count;
            this.initialValue = new DefaultStructInitializer(this, size);
            SaveDataType(typeID, this);
        }

        public bool LookUp(string ident, out FieldSymbol fieldSymbol)
        {
            string name = ident.ToUpper();
            if (this.fieldTable.ContainsKey(name))
            {
                fieldSymbol = this.fieldTable[name];
                return true;
            }
            else
            {
                fieldSymbol = null;
                FieldSymbol undeclField = new FieldSymbol(name, TypeNode.Error);
                this.fieldTable.Add(name, undeclField);
                return false;
            }
        }

        public FieldSymbol FirstField
        {
            get { return this.firstField; }
        }

        public int FieldCount 
        { 
            get { return this.fieldCount; } 
        }

        public override bool IsStructType
        {
            get { return true; }
        }

        public bool IsStoredContiguously
        {
            get { return this.isStoredContiguously; }
        }

        private readonly int fieldCount;

        private readonly FieldSymbol firstField;

        private readonly Dictionary<string, FieldSymbol> fieldTable;

        private readonly bool isStoredContiguously;
    }
}
