using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.ByteCodegenerator;

namespace STLang.Statements
{
    public abstract class Statement : ByteCodeGenerator
    {
        public Statement()
        {
            this.Next = null;
            this.IsFunctionValueDefined = false;
            this.ContainsExit = false;
            this.FunctionReturns = false;
            this.ControlFlowTerminates = false;
        }

        static Statement()
        {
            emptyStatement = new EmptyStatement();
        }

        public abstract void GenerateCode(List<int> exitList = null);

        public virtual bool IsFunctionValueDefined
        {
            get;
            set;
        }

        public virtual bool ContainsExit
        {
            get;
            set;
        }

        public virtual bool FunctionReturns
        {
            get;
            set;
        }

        public virtual bool ControlFlowTerminates
        {
            get;
            set;
        }

        public static Statement Empty
        {
            get { return emptyStatement; }
        }

        public Statement Next { get; set; }

        private static readonly Statement emptyStatement;
    }
}
