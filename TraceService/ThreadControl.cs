using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceBackend.Database;
using TraceBackend.Trace;
using TraceBackend.Utils;
using static TraceBackend.Utils.Logger;

namespace TraceService
{
    public enum ThreadStatus
    {
        NotStarted,
        Running,
        Stopped
    }
    public class ThreadControl
    {
        public delegate void ThreadControlDBInsertDelegate(Exception e);
        public event ThreadControlDBInsertDelegate OnDBInsertError;

        private const int STOP_ATTEMPTS = 5;
        private const int SLEEP_MS = 10000;
        private enum ThreadStatusRequests
        {
            Start,
            Stop,
            None
        }
        public ThreadStatus CurrentStatus { get; private set; }
        private ThreadStatusRequests _requestedStatus;
        private Thread _workThread;
        private DbContext _dbContext;
        private SSASTraceQueryExecution _ssasTraceQueryExecution;

        public ThreadControl(DbContext dbContext, SSASTraceQueryExecution trace)
        {
            _dbContext = dbContext;
            _ssasTraceQueryExecution = trace;
            Init();
        }
        private void Init()
        {
            _requestedStatus = ThreadStatusRequests.None;
            CurrentStatus = ThreadStatus.NotStarted;
            _workThread = null;
        }
        public void RequestStop()
        {
            if (CurrentStatus == ThreadStatus.Stopped)
                return; 

            int attempt;

            Logger.Log("RequestStop(): Started.", LogEntryType.Information);

            _requestedStatus = ThreadStatusRequests.Stop;
            attempt = 1;

            while (CurrentStatus != ThreadStatus.Stopped)
            {
                if (attempt > STOP_ATTEMPTS)
                    break;

                Thread.Sleep(SLEEP_MS);
                Logger.Log(String.Format("RequestStop(): Waiting for CurrentStatus to switch to Stopped. Attempt {0}.", attempt), LogEntryType.Information);

                attempt++;
            }
            CurrentStatus = ThreadStatus.Stopped;
            _workThread = null;
            Logger.Log("RequestStop(): Finished.", LogEntryType.Information);
        }
        public void RequestStart()
        {
            if (CurrentStatus == ThreadStatus.Running)
                return;

            Logger.Log("RequestStart(): Started.", LogEntryType.Information);
            Init();

            _requestedStatus = ThreadStatusRequests.Start;

            _workThread = new Thread(ThreadWork)
            {
                IsBackground = true,
                Name = "Trace Extraction Thread"
            };

            _workThread.Start();
            _requestedStatus = ThreadStatusRequests.None;
            CurrentStatus = ThreadStatus.Running;
            Logger.Log("RequestStart(): Finished.", LogEntryType.Information);
        }
        private void ThreadWork()
        {
            SqlCommand cmd;
            Logger.Log("ThreadWork(): Started.", LogEntryType.Information);

            while (_requestedStatus != ThreadStatusRequests.Stop)
            {
                cmd = _dbContext.TabularQueryCommands.GetInsertCommand();

                CurrentStatus = ThreadStatus.Running;

                bool newRecord = _ssasTraceQueryExecution.ReadNextRecord();

                try
                {

                    cmd.Parameters["@event_class"].Value = _ssasTraceQueryExecution.EventClass;
                    cmd.Parameters["@event_sub_class"].Value = _ssasTraceQueryExecution.EventSubClass;
                    cmd.Parameters["@nt_user_name"].Value = _ssasTraceQueryExecution.NTUserName;
                    cmd.Parameters["@application_name"].Value = (object)_ssasTraceQueryExecution.ApplicationName ?? DBNull.Value;
                    cmd.Parameters["@database_name"].Value = _ssasTraceQueryExecution.DatabaseName;
                    cmd.Parameters["@duration"].Value = _ssasTraceQueryExecution.Duration;
                    cmd.Parameters["@start_time"].Value = _ssasTraceQueryExecution.StartTime;

                    /*StringBuilder query = new StringBuilder();
                    query.AppendLine("ThreadWork(): New interaction detected:");
                    foreach(SqlParameter p in cmd.Parameters)
                    {
                        query.AppendLine(String.Format("{0}: \"{1}\"", p.ParameterName, p.Value.ToString()));
                    }
                    Logger.Log(query.ToString(), EventLogEntryType.Information);*/
                    Logger.Log("ThreadWork(): New interaction detected:", LogEntryType.Information);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    CurrentStatus = ThreadStatus.Stopped;
                    OnDBInsertError(e);
                    return;
                }
            }
            CurrentStatus = ThreadStatus.Stopped;
            Logger.Log("ThreadWork(): Ended.", LogEntryType.Information);
        }
    }
}
