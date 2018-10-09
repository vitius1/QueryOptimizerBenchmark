﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlOptimizerBechmark.Controls.BenchmarkTreeView
{
    public class ConnectionSettingsNode : BenchmarkObjectTreeNode
    {
        public ConnectionSettingsNode(Benchmark.ConnectionSettings connectionSettings, BenchmarkTreeView benchmarkTreeView)
            : base(connectionSettings, benchmarkTreeView)
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
