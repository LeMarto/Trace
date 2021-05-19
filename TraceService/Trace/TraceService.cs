using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraceBackend.Database;
using TraceBackend.Utils;
using TraceBackend.Trace;
using static TraceBackend.Utils.Logger;

namespace TraceService
{
    public partial class TraceService : ServiceBase
    {
        public const int TraceServiceWaitHint = 30000;
        private DbContext _dbContext;
        private SSASTraceQueryExecution _ssasTraceQueryExecution;
        private ThreadControl _thread;
        #region Helper Functions
        private static DirectoryInfo GetExecutingDirectory()
        {
            /*
             * Based on https://www.red-gate.com/simple-talk/blogs/c-getting-the-directory-of-a-running-executable/
             */
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory;
        }
        #endregion
        public TraceService()
        {
            InitializeComponent();

            Logger.LogLevel = Properties.Settings.Default.LogLevel;

            _dbContext = new DbContext();
            _dbContext.OnConnectError += OnDBConnectError;
            _dbContext.OnDisconnectError += OnDBDisconnectError;

            _ssasTraceQueryExecution = new SSASTraceQueryExecution();
            _ssasTraceQueryExecution.OnConnectError += OnTraceConnectError;
            _ssasTraceQueryExecution.OnDisconnectError += OnTraceDisconnectError;
            _ssasTraceQueryExecution.OnReadTraceError += OnTraceReadError;
            _ssasTraceQueryExecution.OnTraceFileNotFoundError += OnTraceFileNotFoundError;
            _ssasTraceQueryExecution.OnFieldAssignmentError += OnFieldAssignmentError;

            _thread = new ThreadControl(_dbContext, _ssasTraceQueryExecution);
            _thread.OnDBInsertError += OnThreadDBInsertError;
        }
        #region Config Loading Functions
        private void LoadTraceConfig()
        {
            string templateFileName = Properties.Settings.Default.TraceTemplateFileName.Replace(".tdf", "").Replace(".TDF", "").Trim();
            string templateFullPath = String.Format("{0}\\Templates\\{1}.tdf", GetExecutingDirectory().FullName, templateFileName);
            templateFullPath = Uri.UnescapeDataString(templateFullPath);
            _ssasTraceQueryExecution.TraceTemplateFilePath = templateFullPath;
            _ssasTraceQueryExecution.TraceServer = Properties.Settings.Default.TraceServer;
        }
        private void LoadDatabaseConfig()
        {
            _dbContext.ServerName = Properties.Settings.Default.DatabaseServer;
        }
        private void LoadConfig()
        {
            LoadTraceConfig();
            LoadDatabaseConfig();
        }
        #endregion
        #region Error Handling Events
        private void OnFieldAssignmentError(string fieldName, Exception e)
        {
            Logger.Log(String.Format("Error while assigning value of field \"{0}\".\nException Message: \"{1}\".\nInner Exception Message:\"{2}\".", fieldName, e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        private void OnTraceFileNotFoundError()
        {
            Logger.Log(String.Format("Trace template file not found: \"{0}\".", _ssasTraceQueryExecution.TraceTemplateFilePath), LogEntryType.Error);
            OnStop();
        }
        private void OnTraceReadError(Exception e)
        {
            Logger.Log(String.Format("Error while reading the trace.\nException Message: \"{0}\".\nInner Exception Message:\"{1}\".", e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        private void OnTraceDisconnectError(Exception e)
        {
            Logger.Log(String.Format("Error while disconnecting the trace pointing at \"{0}\".\nException Message: \"{1}\".\nInner Exception Message:\"{2}\".", _ssasTraceQueryExecution.TraceServer, e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        private void OnTraceConnectError(Exception e)
        {
            Logger.Log(String.Format("Error while connecting the trace pointing at \"{0}\".\nException Message: \"{1}\".\nInner Exception Message:\"{2}\".", _ssasTraceQueryExecution.TraceServer, e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        private void OnDBDisconnectError(Exception e)
        {
            Logger.Log(String.Format("Error while connecting to the logging database pointing at \"{0}\".\nException Message: \"{1}\".\nInner Exception Message:\"{2}\".", _dbContext.ServerName, e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        private void OnDBConnectError(Exception e)
        {
            Logger.Log(String.Format("Error while disconnecting to the logging database pointing at \"{0}\".\nException Message: \"{1}\".\nInner Exception Message:\"{2}\".", _dbContext.ServerName, e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }

        private void OnThreadDBInsertError(Exception e)
        {
            Logger.Log(String.Format("Error while inserting to the logging database.\nException Message: \"{0}\".\nInner Exception Message:\"{1}\".", e.Message, e.InnerException.Message), LogEntryType.Error);
            OnStop();
        }
        #endregion
        #region Service Methods
        protected override void OnStart(string[] args)
        {
            LoadConfig();
            if (_dbContext.Connect())
            {
                if (_ssasTraceQueryExecution.Connect())
                    _thread.RequestStart();
                else
                    return;
            }
            else
                return;
       }
        protected override void OnStop()
        {
            _thread.RequestStop();
            _ssasTraceQueryExecution.Disconnect();
            _dbContext.Disconnect();
        }
        #endregion

    }
}
