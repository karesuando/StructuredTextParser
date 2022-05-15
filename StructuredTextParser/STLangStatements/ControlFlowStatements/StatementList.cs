using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.ErrorManager;
using STLang.ByteCodegenerator;

namespace STLang.Statements
{
    public class StatementList : ByteCodeGenerator
    {
        public StatementList()
        {
           this.statementList = new List<Statement>();
           this.controlFlowTerminates = false;
           this.functionValueDefined = false;
           this.containsExit = false;
           this.functionReturns = false;
        }

        static StatementList()
        {
            emptyStatList = new StatementList();
        }

        public void Clear()
        {
            this.statementList.Clear();
        }

        public void Add(Statement statement)
        {
            if (statement != null && statement != Statement.Empty)
            {
                this.statementList.Add(statement);
                if (!this.containsExit)
                    this.containsExit = statement.ContainsExit;
                if (!this.functionReturns)
                    this.functionReturns = statement.FunctionReturns;
                if (! this.functionValueDefined && ! this.containsExit)
                    this.functionValueDefined = statement.IsFunctionValueDefined;
                if (!this.controlFlowTerminates)
                    this.controlFlowTerminates = statement.ControlFlowTerminates;
            }
        }

        public void GenerateCode(List<int> exitList = null)
        {
            foreach (Statement statement in this.statementList)
                statement.GenerateCode(exitList);
        }

        public int Count
        {
            get { return this.statementList.Count; }
        }

        public Statement Last
        {
            get
            {
                if (this.statementList.Count == 0)
                    throw new STLangCompilerError("Statement list is empty.");
                else
                    return this.statementList.Last();
            }
        }

        public bool ContainsExit
        {
            get { return this.containsExit; }
        }

        public bool IsFunctionValueDefined
        {
            get {return this.functionValueDefined;}
        }

        public bool POUReturns
        {
            get { return this.functionReturns; }
        }

        public bool ControlFlowTerminates
        {
            get { return this.controlFlowTerminates; }
        }

        public static StatementList Empty 
        { 
            get { return emptyStatList; } 
        }

        private List<Statement> statementList;

        private static readonly StatementList emptyStatList;

        private bool functionValueDefined;

        private bool containsExit;

        private bool functionReturns;

        private bool controlFlowTerminates;
    }
}
