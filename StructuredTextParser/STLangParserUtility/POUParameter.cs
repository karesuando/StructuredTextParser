using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using QUT.Gppg;
using STLang.Parser;
using STLang.Symbols;
using STLang.Expressions;

namespace STLang.ParserUtility
{
    public abstract class POUParameter : Expression
    {
        public POUParameter(Expression expression, LexLocation loc)
            : base(expression.DataType, expression.ToString())
        {
            this.formalName = "";
            this.value = expression;
            this.location = loc;
        }

        public POUParameter(string name, Expression expression, string assignStr, LexLocation loc)
            : base(expression.DataType, name + assignStr + expression)
        {
            this.formalName = name;
            this.value = expression;
            this.location = loc;
        }

        public override void GenerateLoad(List<int> trueBranch, List<int> falseBranch)
        {
            this.value.GenerateLoad();
        }

        public string FormalName
        {
            get { return this.formalName; }
        }

        public int Position
        {
            get;
            set;
        }

        public virtual bool IsInputArgAssignment
        {
            get { return false; }
        }

        public virtual Expression LValue
        {
            get { throw new NotImplementedException("getter LValue not implemented"); }
            set { throw new NotImplementedException("setter LValue not implemented"); }
        }

        public virtual bool IsInputParameter
        {
            get { return false; }
        }

        public virtual bool IsOutputParameter
        {
            get { return false; }
        }

        public Expression RValue
        {
            get { return this.value; }
        }

        public virtual bool IsInvertedOutput
        {
            get { return false; }
        }

        public LexLocation LexicalLocation
        {
            get { return this.location; }
        }

        private readonly string formalName;

        private readonly Expression value;

        private readonly LexLocation location;

        private readonly bool isInputArgAssign;
    }
}
