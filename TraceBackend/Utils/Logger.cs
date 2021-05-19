using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TraceBackend.Utils
{
    public static class Logger
    {
        public enum LogEntryType
        {
            Error,
            Warning,
            Information
        }
        #region Event Log Constants
        public const string LOG_SOURCE = "Trace";
        public const string LOG_NAME = "Application";
        public static int LogLevel = 0;
        #endregion
        private static EventLog _eventLog;
        public static void Log(string message, LogEntryType type)
        {
            if (_eventLog == null)
                InitEventLog();

            if ((int)type > LogLevel)
                return;

            string entryTypeText="";
            EventLogEntryType callType= EventLogEntryType.Information;

            if (type == LogEntryType.Error)
            {
                entryTypeText = "Error";
                callType = EventLogEntryType.Error;
            }
            else if (type == LogEntryType.Information)
            {
                entryTypeText = "Information";
                callType = EventLogEntryType.Information;
            }
            else if (type == LogEntryType.Warning)
            {
                entryTypeText = "Warning";
                callType = EventLogEntryType.Warning;
            }

            Console.WriteLine(String.Format("{0}: {1}", entryTypeText, message));
            _eventLog.WriteEntry(message, callType);
        }
        private static void InitEventLog()
        {
            if (!EventLog.SourceExists(LOG_SOURCE))
                EventLog.CreateEventSource(LOG_SOURCE, LOG_NAME);

            _eventLog = new EventLog();
            _eventLog.Source = LOG_SOURCE;
            _eventLog.Log = LOG_NAME;
        }
    }

}
