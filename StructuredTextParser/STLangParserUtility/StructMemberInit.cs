using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Expressions;

namespace STLang.ParserUtility
{
    public class StructMemberInit : Expression
    {
        public StructMemberInit(string member, Expression initValue)
            : base(initValue.DataType, member + " := " + initValue)
        {
            this.member = member;
            this.initValue = initValue;
        }

        public string Member
        {
            get { return this.member; }
        }

        public Expression InitValue
        {
            get { return this.initValue; }
        }

        private readonly string member;

        private readonly Expression initValue;
    }
}
