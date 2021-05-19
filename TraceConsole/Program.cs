using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceBackend.Database;
using TraceBackend.Trace;

using System.Threading;

namespace TraceConsole
{
    class Program
    {
        private DbContext _dbContext;
        private SSASTraceQueryExecution _ssasTraceQueryExecution;
        private SqlCommand cmd;
        private Thread thread;
        bool wantToEndThread;
        bool wantToPauseThread;
        bool myEvent;
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }
        public Program()
        {
            string templateFileName = Properties.Settings.Default.TraceTemplateFileName.Replace(".tdf", "").Replace(".TDF", "").Trim();

            _dbContext = new DbContext();
            _dbContext.ServerName = Properties.Settings.Default.DatabaseServer;

            _ssasTraceQueryExecution = new SSASTraceQueryExecution();
            _ssasTraceQueryExecution.TraceServer = Properties.Settings.Default.TraceServer;
            _ssasTraceQueryExecution.TraceTemplateFilePath = String.Format(".\\Templates\\{0}", templateFileName);
        }
        public Thread GetThread()
        {
            return new Thread(ThreadWork)
            {
                IsBackground = true,
                Name = "Trace Fetcher"
            };
        }
        public void Start()
        {
            _dbContext.Connect();
            _ssasTraceQueryExecution.Connect();
            cmd = _dbContext.TabularQueryCommands.GetInsertCommand();
            wantToEndThread = false;
            wantToPauseThread = false;
            myEvent = false;

            //thread.Start();
            while (true)
            {
                string option = Console.ReadLine();
                switch (option)
                {
                    case "stop":
                        wantToEndThread = true;
                        break;
                    case "pause":
                        wantToPauseThread = true;
                        break;
                    case "resume":
                        wantToPauseThread = false;
                        break;
                    case "start":
                        wantToEndThread = false;
                        wantToPauseThread = false;
                        thread = GetThread();
                        thread.Start();
                        break;
                    case "event":
                        myEvent = true;
                        break;
                    case "status":
                        Console.WriteLine(String.Format("Thread Status: {0}", thread.ThreadState.ToString()));
                        break;
                }
            }
        }
        public void ExternalEvent(string msg)
        {
            Console.WriteLine(String.Format("This is an external event: {0}",  msg));
        }
        public void ThreadWork()
        {
            Console.WriteLine("ThreadWork()->Start");
            while (!wantToEndThread)
            {
                if (myEvent)
                {
                    ExternalEvent("blah");
                    myEvent = false;
                }
                if (wantToPauseThread)
                {
                    while (wantToPauseThread)
                    {
                        Console.WriteLine("ThreadWork()->Paused");
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine("ThreadWork()->Resumed");
                }

                bool haveRecords = _ssasTraceQueryExecution.ReadNextRecord();

                if (!haveRecords)
                    continue;

                Console.Write("Captured an Event, writing to database...");

                cmd.Parameters["@event_class"].Value = _ssasTraceQueryExecution.EventClass;
                cmd.Parameters["@event_sub_class"].Value = _ssasTraceQueryExecution.EventSubClass;
                cmd.Parameters["@nt_user_name"].Value = _ssasTraceQueryExecution.NTUserName;
                cmd.Parameters["@application_name"].Value = (object)_ssasTraceQueryExecution.ApplicationName ?? DBNull.Value;
                cmd.Parameters["@database_name"].Value = _ssasTraceQueryExecution.DatabaseName;
                cmd.Parameters["@text_data"].Value = _ssasTraceQueryExecution.TextData;
                cmd.Parameters["@duration"].Value = _ssasTraceQueryExecution.Duration;
                cmd.Parameters["@start_time"].Value = _ssasTraceQueryExecution.StartTime;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error!\nException Message: \"{0}\"", e.Message);
                    break;
                }
                Console.WriteLine("Done!");
            }
            Console.WriteLine("ThreadWork()->End");
        }

    }
}
