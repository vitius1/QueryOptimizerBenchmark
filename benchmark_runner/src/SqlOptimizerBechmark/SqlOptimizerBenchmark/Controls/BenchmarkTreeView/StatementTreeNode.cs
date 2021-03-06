﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlOptimizerBenchmark.Controls.BenchmarkTreeView
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
            ChildrenBound = true;
        }

        public override bool HasChildren()
        {
            return false;
        }
    }
}
