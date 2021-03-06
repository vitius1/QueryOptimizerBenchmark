﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.H2;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SqlOptimizerBenchmark.Benchmark;

namespace SqlOptimizerBenchmark.DbProviders.H2
{
    public class H2Provider : DbProvider
    {
        #region Fields

        private bool useConnectionString = false;
        private string url = string.Empty;
        private string user = string.Empty;
        private string password = string.Empty;
        private int commandTimeout;
        private string connectionString = string.Empty;

        private H2Connection connection;

        #endregion

        public override string GetSettingsInfo()
        {
            if (!useConnectionString)
            {
                return $"dbms=H2|url={url}|user={user}|password={password}|commandTimeout={commandTimeout}";
            }
            else
            {
                return $"dbms=H2|connectionString={connectionString}|commandTimeout={commandTimeout}";
            }
        }

        #region Properties

        public override string Name => "H2";

        public bool UseConnectionString
        {
            get => useConnectionString;
            set => useConnectionString = value;
        }
        
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }

        public string ConnectionString
        {
            get => connectionString;
            set => connectionString = value;
        }

        #endregion

        public override void Connect()
        {
            connection = new H2Connection();
            connection.ConnectionString = GetConnectionString();
            connection.Open();
        }

        private void CheckConnection()
        {
            try
            {
                H2Command cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
            }
            catch (H2Exception h2Ex)
            {
                // Force to create a new connection.
                if (h2Ex.Message.ToLower().Contains("the database has been closed"))
                {
                    connection = null;
                }
                else
                {
                    throw;
                }
            }

            if (connection == null || !connection.IsOpen)
            {
                Connect();
            }
        }

        public override void Close()
        {
            connection.Close();
        }

        public string GetConnectionString()
        {
            return string.Format("{0};USER={1};PASSWORD={2}", url, user, password);
        }

        public override DbProviderSettingsControl CreateSettingsControl()
        {
            H2SettingsControl settingsControl = new H2SettingsControl();
            settingsControl.Provider = this;
            return settingsControl;
        }

        public override void Execute(string statement)
        {
            CheckConnection();

            H2Command cmd = connection.CreateCommand();
            cmd.CommandTimeout = commandTimeout;
            cmd.CommandText = statement;
            cmd.ExecuteNonQuery();
        }

        public override DataTable ExecuteQuery(string query)
        {
            DataTable table = new DataTable();
            H2Command command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
            using (H2DataReader reader = command.ExecuteReader())
            {
                table.Load(reader);
            }
            return table;
        }

        public override QueryPlan GetQueryPlan(string query)
        {
            CheckConnection();

            H2Command cmd = connection.CreateCommand();
            cmd.CommandText = "EXPLAIN PLAN FOR " + query;
            object planObj = cmd.ExecuteScalar();
            if (planObj != null)
            {
                string planStr = planObj.ToString();
                QueryPlan plan = new QueryPlan();
                QueryPlanNode root = new QueryPlanNode();
                root.OpName = planStr;
                plan.Root = root;
                return plan;
            }
            else
            {
                return null;
            }
        }

        public override QueryStatistics GetQueryStatistics(string query, bool retrieveWholeResult)
        {
            H2DataReader reader = null;
            try
            {
                CheckConnection();

                QueryStatistics ret = new QueryStatistics();

                H2Command cmdQuery = connection.CreateCommand();
                cmdQuery.CommandText = query;
                cmdQuery.CommandTimeout = commandTimeout;

                DateTime t0 = DateTime.Now;

                int resultSize = 0;
                if (!retrieveWholeResult)
                {
                    reader = cmdQuery.ExecuteReader();
                    while (reader.Read())
                    {
                        resultSize++;
                    }
                    reader.Close();
                    reader = null;
                }
                else
                {
                    DataTable dataTable = new DataTable();

                    reader = cmdQuery.ExecuteReader();

                    bool firstRead = true;
                    while (reader.Read())
                    {
                        if (firstRead)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = string.Format("col_{0}", i);
                                DataColumn column = new DataColumn(columnName);
                                dataTable.Columns.Add(column);
                            }
                            firstRead = false;
                        }

                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader[i];
                        }
                        dataTable.Rows.Add(row);
                    }
                    reader.Close();
                    reader = null;

                    ret.Result = dataTable;
                    resultSize = ret.Result.Rows.Count;
                }

                DateTime t1 = DateTime.Now;
                
                ret.QueryProcessingTime = t1 - t0;
                ret.ResultSize = resultSize;

                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }
        }

        public override void LoadFromXml(XElement element)
        {
            useConnectionString = Convert.ToBoolean(element.Attribute("use_connection_string").Value);
            url = element.Attribute("url").Value;
            user = element.Attribute("user_name").Value;
            password = element.Attribute("password").Value;            
            connectionString = element.Attribute("connection_string").Value;
            commandTimeout = Convert.ToInt32(element.Attribute("command_timeout").Value);
        }

        public override void SaveToXml(XElement element)
        {
            element.Add(new XAttribute("use_connection_string", useConnectionString));
            element.Add(new XAttribute("url", url));
            element.Add(new XAttribute("user_name", user));
            element.Add(new XAttribute("password", password));
            element.Add(new XAttribute("connection_string", connectionString));
            element.Add(new XAttribute("command_timeout", commandTimeout));
        }

        public override void ExportToFileSystem(string path, Benchmark.Benchmark benchmark)
        {

        }

        public override string GetTestingScript(Benchmark.Benchmark benchmark)
        {
            return null;
        }
    }
}
