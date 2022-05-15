using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.Symbols;
using QUT.Gppg;
using STLang.Expressions;
using STLang.ErrorManager;

namespace STLang.ParserUtility
{
    public abstract class ProgramOrganizationUnitCall
    {
        public ProgramOrganizationUnitCall(STLangSymbol pouSymbol, LexLocation location)
        {
            this.pouSymbol = pouSymbol;
            this.location = location;
            this.arguments = new List<Expression>();
            this.isInputArgAssignment = false;
        }

        public ProgramOrganizationUnitCall(STLangSymbol pouSymbol, POUParameter argument, LexLocation location)
        {
            this.pouSymbol = pouSymbol;
            this.location = location;
            this.arguments = new List<Expression>();
            this.Add(argument);
            this.isInputArgAssignment = argument.IsInputArgAssignment;
        }

        public virtual void Add(POUParameter argument)
        {
            if (argument != null)
                this.arguments.Add(argument);
        }

        public STLangSymbol Symbol
        {
            get { return this.pouSymbol; }
        }

        public virtual bool IsFunctionBlock
        {
            get { return false; }
        }

        public virtual bool IsFunction
        {
            get { return false; }
        }

        public string Name
        {
            get { return this.pouSymbol.Name; }
        }

        public bool IsInputArgAssignment
        {
            get { return this.isInputArgAssignment; }
        }

        public LexLocation Location
        {
            get { return this.location; }
        }

        public Expression MakeSyntaxTreeNode()
        {
            return this.pouSymbol.MakeSyntaxTreeNode(this.arguments, this.location);
        }

        protected readonly STLangSymbol pouSymbol;

        protected readonly List<Expression> arguments;

        protected readonly LexLocation location;

        private readonly bool isInputArgAssignment;
    }
}
