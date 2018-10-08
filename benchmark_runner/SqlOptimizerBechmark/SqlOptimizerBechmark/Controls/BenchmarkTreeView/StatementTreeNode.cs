﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlOptimizerBechmark.Controls.BenchmarkTreeView
{
    public class StatementTreeNode : BenchmarkObjectTreeNode
    {
        public StatementTreeNode(Benchmark.Statement statement, BenchmarkTreeView benchmarkTreeView)
            : base(statement, benchmarkTreeView)
        {
        }

        public override void BindNode()
        {

        }

        public override void BindChildren()
        {
        }

        public override bool HasChildren()
        {
            return false;
        }
    }
}
