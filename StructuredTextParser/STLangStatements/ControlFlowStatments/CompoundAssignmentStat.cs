using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.Statements
{
    public class CompoundAssignmentStat : AssignmentStat
    {
        public CompoundAssignmentStat()
        {
            this.assignmentStats = new List<Statement>();
        }

        public void Add(Statement assignStat)
        {
            if (assignStat != null)
                this.assignmentStats.Add(assignStat);
        }

        public override void GenerateCode(List<int> exitList = null)
        {
            foreach (Statement assignmentStat in this.assignmentStats)
                assignmentStat.GenerateCode();
        }

        private readonly List<Statement> assignmentStats;
    }
}
