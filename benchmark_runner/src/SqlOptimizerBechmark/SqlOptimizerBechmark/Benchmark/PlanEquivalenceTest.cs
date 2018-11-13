﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlOptimizerBechmark.Benchmark
{
    public class PlanEquivalenceTest: Test
    {
        private ObservableCollection<QueryVariant> variants = new ObservableCollection<QueryVariant>();
        private int expectedResultSize = 0;

        public override IEnumerable<IBenchmarkObject> ChildObjects
        {
            get
            {
                foreach (QueryVariant variant in variants)
                {
                    yield return variant;
                }
            }
        }

        public override TestType TestType => TestType.PlanEquivalence;

        public ObservableCollection<QueryVariant> Variants
        {
            get => variants;
        }

        public int ExpectedResultSize
        {
            get => expectedResultSize;
            set
            {
                if (value != expectedResultSize)
                {
                    expectedResultSize = value;
                    OnPropertyChanged("ExpectedResultSize");
                }
            }
        }

        public PlanEquivalenceTest(TestGroup testGroup)
            : base(testGroup)
        {
            variants.CollectionChanged += Variants_CollectionChanged;
        }

        private void Variants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyChange();
        }

        public override void SaveToXml(BenchmarkXmlSerializer serializer)
        {
            base.SaveToXml(serializer);
            serializer.WriteCollection<QueryVariant>("variants", "variant", variants);
            serializer.WriteInt("expected_result_size", expectedResultSize);
        }

        public override void LoadFromXml(BenchmarkXmlSerializer serializer)
        {
            base.LoadFromXml(serializer);
            serializer.ReadCollection<QueryVariant>("variants", "variant", variants,
                delegate () { return new QueryVariant(this); });
            serializer.ReadInt("expected_result_size", ref expectedResultSize);
        }
    }
}