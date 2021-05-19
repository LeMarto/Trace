using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceBackend.Database.CommandFactory;
using TraceBackend.Utils;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Trace;
using static TraceBackend.Utils.Logger;

namespace TraceBackend.Trace
{
    public class SSASTraceQueryExecution : SSASTrace
    {
        private enum EnumFields
        {
            SPID,
            EventClass,
            EventSubClass,
            DatabaseName,
            NTUserName,
            ApplicationName,
            TextData,
            StartTime,
            Duration            
        };
        #region Fields
        public string EventClass { get; private set; }
        public int EventSubClass { get; private set; }
        public string DatabaseName { get; private set; }
        public string NTUserName { get; private set; }
        public string ApplicationName { get; private set; }
        public string TextData { get; private set; }
        public DateTime StartTime { get; private set; }
        public long Duration { get; private set; }
        #endregion

        public SSASTraceQueryExecution() : base()
        {

        }

        public bool ReadNextRecord()
        {
            bool traceContainsData = false;

            Logger.Log("Trace:ReadNextRecord(): Waiting for event...", LogEntryType.Information);

            try
            {
                traceContainsData = _traceServer.Read();
            }
            catch (Exception e)
            {
                TriggerOnReadTraceError(e);
                return false;
            }

            if (!traceContainsData)
                return false;

            for (int i = 0; i < Enum.GetNames(typeof(EnumFields)).Length; i++)
            {
                try
                {
                    switch ((EnumFields)i)
                    {
                        case EnumFields.SPID:
                            break;
                        case EnumFields.EventClass:
                            EventClass = _traceServer.GetString(i);
                            break;
                        case EnumFields.EventSubClass:
                            EventSubClass = _traceServer.GetInt32(i);
                            break;
                        case EnumFields.NTUserName:
                            NTUserName = _traceServer.GetString(i);
                            break;
                        case EnumFields.ApplicationName:
                            ApplicationName = _traceServer[i] == null ? null : _traceServer.GetString(i);
                            break;
                        case EnumFields.DatabaseName:
                            DatabaseName = _traceServer.GetString(i);
                            break;
                        case EnumFields.TextData:
                            TextData = _traceServer.GetString(i);
                            break;
                        case EnumFields.Duration:
                            Duration = _traceServer.GetInt64(i);
                            break;
                        case EnumFields.StartTime:
                            StartTime = _traceServer.GetDateTime(i);
                            break;
                    }
                }
                catch (Exception e)
                {
                    TriggerOnFieldAssignmentError(((EnumFields)i).ToString(), e);
                    return false;
                }
            }
            return true;
        }

    }
}
