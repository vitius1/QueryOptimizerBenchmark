﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlOptimizerBechmark.Executor
{
    public class Executor
    {
        private static Executor instance = new Executor();
        private Benchmark.Benchmark benchmark;
        private Benchmark.TestRun testRun;
        private Thread testingThread = null;
        private bool runInitScript = true;
        private bool runCleanUpScript = true;

        private volatile bool stopTesting = false;
        private volatile bool interruptTesting = false;

        public static Executor Instance => instance;

        public Benchmark.Benchmark Benchmark => benchmark;

        public Benchmark.TestRun TestRun => testRun;

        public bool Testing => testingThread != null;


        public event EventHandler<ExecutorMessageEventArgs> Message;
        
        protected virtual void OnMessage(string message, ExecutorMessageType messageType)
        {
            if (Message != null)
            {
                Message(this, new ExecutorMessageEventArgs(message, messageType));
            }
        }

        public event EventHandler TestingStarted;

        protected virtual void OnTestingStarted()
        {
            if (TestingStarted != null)
            {
                TestingStarted(this, EventArgs.Empty);
            }
        }

        public event EventHandler TestingEnded;

        protected virtual void OnTestingEnded()
        {
            if (TestingEnded != null)
            {
                TestingEnded(this, EventArgs.Empty);
            }
        }

        public void Prepare(string name)
        {
            if (benchmark == null)
            {
                throw new Exception("Benchmark is not set.");
            }

            testRun = new Benchmark.TestRun(benchmark);
            testRun.Name = name;

            foreach (Benchmark.TestGroup testGroup in benchmark.TestGroups)
            {
                Benchmark.TestGroupResult testGroupResult = new Benchmark.TestGroupResult(testRun);
                testGroupResult.TestGroupId = testGroup.Id;
                testGroupResult.TestGroupName = testGroup.Name;
                testRun.TestGroupResults.Add(testGroupResult);

                foreach (Benchmark.Configuration configuration in testGroup.Configurations)
                {
                    Benchmark.ConfigurationResult configurationResult = new Benchmark.ConfigurationResult(testRun);
                    configurationResult.ConfigurationId = configuration.Id;
                    configurationResult.ConfigurationName = configuration.Name;
                    testRun.ConfigurationResults.Add(configurationResult);

                    foreach (Benchmark.Test test in testGroup.Tests)
                    {
                        if (!test.Active)
                        {
                            continue;
                        }

                        if (test is Benchmark.PlanEquivalenceTest planEquivalenceTest)
                        {
                            Benchmark.PlanEquivalenceTestResult planEquivalenceTestResult = new Benchmark.PlanEquivalenceTestResult(testRun);
                            planEquivalenceTestResult.TestId = test.Id;
                            planEquivalenceTestResult.TestName = test.Name;
                            planEquivalenceTestResult.TestGroupId = testGroup.Id;
                            planEquivalenceTestResult.ConfigurationId = configuration.Id;
                            testRun.TestResults.Add(planEquivalenceTestResult);

                            foreach (Benchmark.QueryVariant variant in planEquivalenceTest.Variants)
                            {
                                Benchmark.QueryVariantResult queryVariantResult = new Benchmark.QueryVariantResult(planEquivalenceTestResult);
                                queryVariantResult.Query = variant.Statement.CommandText;
                                queryVariantResult.QueryVariantId = variant.Id;
                                queryVariantResult.QueryVariantName = variant.Name;
                                planEquivalenceTestResult.QueryVariantResults.Add(queryVariantResult);
                            }
                        }
                        // TODO - other test types.
                    }
                }
            }

            benchmark.TestRuns.Add(testRun);
        }

        private void Execute()
        {
            try
            {
                DbProviders.DbProvider db = benchmark.ConnectionSettings.DbProvider;
                db.Connect();

                try
                {
                    // Init script.
                    foreach (Benchmark.Statement statement in benchmark.InitScript.Statements)
                    {
                        if (interruptTesting)
                        {
                            testingThread = null;
                            OnTestingEnded();
                            return;
                        }

                        string commandText = statement.CommandText;
                        db.Execute(commandText);
                        OnMessage(commandText, ExecutorMessageType.StatementCompleted);
                    }
                }
                catch (Exception ex)
                {
                    OnMessage(ex.Message, ExecutorMessageType.Error);
                    testingThread = null;
                    OnTestingEnded();
                    return;
                }

                try
                {
                    foreach (Benchmark.TestGroup testGroup in benchmark.TestGroups)
                    {
                        if (interruptTesting)
                        {
                            testingThread = null;
                            OnTestingEnded();
                            return;
                        }
                        if (stopTesting)
                        {
                            break;
                        }

                        bool activeTests = false;
                        foreach (Benchmark.Test test in testGroup.Tests)
                        {
                            if (test.Active)
                            {
                                activeTests = true;
                                break;
                            }
                        }

                        if (activeTests)
                        {
                            foreach (Benchmark.Configuration configuration in testGroup.Configurations)
                            {
                                if (interruptTesting)
                                {
                                    testingThread = null;
                                    OnTestingEnded();
                                    return;
                                }
                                if (stopTesting)
                                {
                                    break;
                                }

                                try
                                {
                                    // Init script.
                                    foreach (Benchmark.Statement statement in configuration.InitScript.Statements)
                                    {
                                        if (interruptTesting)
                                        {
                                            testingThread = null;
                                            OnTestingEnded();
                                            return;
                                        }

                                        string commandText = statement.CommandText;
                                        db.Execute(commandText);
                                        OnMessage(commandText, ExecutorMessageType.StatementCompleted);
                                    }

                                    try
                                    {
                                        foreach (Benchmark.Test test in testGroup.Tests)
                                        {
                                            if (!test.Active)
                                            {
                                                continue;
                                            }

                                            if (interruptTesting)
                                            {
                                                testingThread = null;
                                                OnTestingEnded();
                                                return;
                                            }
                                            if (stopTesting)
                                            {
                                                break;
                                            }

                                            Benchmark.TestResult currentTestResult = null;
                                            foreach (Benchmark.TestResult testResult in testRun.TestResults)
                                            {
                                                if (testResult.TestId == test.Id &&
                                                    testResult.ConfigurationId == configuration.Id)
                                                {
                                                    currentTestResult = testResult;
                                                    break;
                                                }
                                            }
                                            if (currentTestResult != null)
                                            {
                                                if (currentTestResult is Benchmark.PlanEquivalenceTestResult planEquivalenceTestResult)
                                                {
                                                    planEquivalenceTestResult.Started = true;
                                                    HashSet<string> distinctPlans = new HashSet<string>();

                                                    foreach (Benchmark.QueryVariantResult queryVariantResult in planEquivalenceTestResult.QueryVariantResults)
                                                    {
                                                        try
                                                        {
                                                            if (interruptTesting)
                                                            {
                                                                testingThread = null;
                                                                OnTestingEnded();
                                                                return;
                                                            }
                                                            if (stopTesting)
                                                            {
                                                                break;
                                                            }

                                                            queryVariantResult.Started = true;

                                                            DbProviders.QueryStatistics stats = db.GetQueryStatistics(queryVariantResult.Query);
                                                            string plan = db.GetQueryPlan(queryVariantResult.Query);

                                                            queryVariantResult.QueryProcessingTime = stats.QueryProcessingTime;
                                                            queryVariantResult.ResultSize = stats.ResultSize;
                                                            queryVariantResult.QueryPlan = plan;

                                                            if (!distinctPlans.Contains(plan))
                                                            {
                                                                distinctPlans.Add(plan);
                                                            }

                                                            OnMessage(queryVariantResult.Query, ExecutorMessageType.QueryExecuted);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            queryVariantResult.ErrorMessage = ex.Message;
                                                            OnMessage(ex.Message, ExecutorMessageType.Error);
                                                        }
                                                        queryVariantResult.Completed = true;
                                                    }

                                                    planEquivalenceTestResult.DistinctQueryPlans = distinctPlans.Count;
                                                    planEquivalenceTestResult.Completed = true;
                                                    OnMessage(string.Format("Test {0} completed.", planEquivalenceTestResult.TestName), ExecutorMessageType.TestCompleted);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnMessage(ex.Message, ExecutorMessageType.Error);
                                    }

                                    // Clean up script.
                                    foreach (Benchmark.Statement statement in configuration.CleanUpScript.Statements)
                                    {
                                        if (interruptTesting)
                                        {
                                            testingThread = null;
                                            OnTestingEnded();
                                            return;
                                        }

                                        string commandText = statement.CommandText;
                                        db.Execute(commandText);
                                        OnMessage(commandText, ExecutorMessageType.StatementCompleted);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    OnMessage(ex.Message, ExecutorMessageType.Error);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnMessage(ex.Message, ExecutorMessageType.Error);
                }

                try
                {
                    // Clean up script.
                    foreach (Benchmark.Statement statement in benchmark.CleanUpScript.Statements)
                    {
                        if (interruptTesting)
                        {
                            testingThread = null;
                            OnTestingEnded();
                            return;
                        }

                        string commandText = statement.CommandText;
                        db.Execute(commandText);
                        OnMessage(commandText, ExecutorMessageType.StatementCompleted);
                    }
                }
                catch (Exception ex)
                {
                    OnMessage(ex.Message, ExecutorMessageType.Error);
                }

                db.Close();
            }
            catch (Exception ex)
            {
                OnMessage(ex.Message, ExecutorMessageType.Error);
            }

            testingThread = null;
            OnTestingEnded();
        }

        public void StartTesting()
        {
            testingThread = new Thread(Execute);
            testingThread.Start();
            OnTestingStarted();
            stopTesting = false;
        }

        public void StopTesting()
        {
            OnMessage(string.Empty, ExecutorMessageType.UserCancelled);
            stopTesting = true;
        }

        public void InterruptTesting()
        {
            OnMessage(string.Empty, ExecutorMessageType.UserInterrupt);
            interruptTesting = true;
        }

        public void LaunchTest(Benchmark.Benchmark benchmark)
        {
            this.benchmark = benchmark;

            if (benchmark.ConnectionSettings.DbProvider == null)
            {
                MessageBox.Show("Database connection is not set.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DateTime now = DateTime.Now;

            Controls.NewTestRunDialog dialog = new Controls.NewTestRunDialog();
            dialog.TestRunName = now.ToString("yyyy-MM-dd HH:mm:ss");
            dialog.RunInitScript = runInitScript;
            dialog.RunCleanUpScript = runCleanUpScript;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Prepare(dialog.TestRunName);
                runInitScript = dialog.RunInitScript;
                runCleanUpScript = dialog.RunCleanUpScript;

                StartTesting();
            }

        }
    }
}
