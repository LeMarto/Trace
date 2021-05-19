using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceBackend.Utils;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Trace;
using static TraceBackend.Utils.Logger;

namespace TraceBackend.Trace
{
    public abstract class SSASTrace : IDisposable
    {
        public delegate void SSASTraceException(Exception e);
        public delegate void SSASTraceError();
        public delegate void SSASTraceFieldAssignmentException(string fieldName, Exception e);

        public event SSASTraceException OnConnectError;
        public event SSASTraceException OnDisconnectError;
        public event SSASTraceException OnReadTraceError;
        public event SSASTraceError OnTraceFileNotFoundError;
        public event SSASTraceFieldAssignmentException OnFieldAssignmentError;

        protected TraceServer _traceServer;
        private OlapConnectionInfo _connectionInfo;
        public string TraceServer
        {
            set { _connectionInfo.ServerName = value; }
            get { return _connectionInfo.ServerName; }
        }
        public string TraceTemplateFilePath { get; set; }
        protected static bool FileExists(string pathToFile)
        {
            return File.Exists(pathToFile);
        }
        public SSASTrace()
        {
            _connectionInfo = new OlapConnectionInfo()
            {
                //ServerName = traceServer,
                UseIntegratedSecurity = true,
                ApplicationName = "Trace"
            };
            Init();
        }
        private void Init()
        {
            _traceServer = new TraceServer();
        }
        protected void TriggerOnReadTraceError(Exception e)
        {
            OnReadTraceError(e);
        }
        protected void TriggerOnFieldAssignmentError(string fieldName, Exception e)
        {
            OnFieldAssignmentError(fieldName, e);
        }
        public bool Connect()
        {
            Logger.Log("Trace:Connect(): Started", LogEntryType.Information);
            if (!FileExists(TraceTemplateFilePath))
            {
                OnTraceFileNotFoundError();
                return false;
            }
            try
            {
                _traceServer.InitializeAsReader(_connectionInfo, TraceTemplateFilePath);
            }
            catch (Exception e)
            {
                OnConnectError(e);
                return false;
            }
            Logger.Log("Trace:Connect(): Finished", LogEntryType.Information);
            return true;
        }
        public bool Disconnect()
        {
            Logger.Log("Trace:Disconnect(): Started", LogEntryType.Information);
            try
            {
                if (!_traceServer.IsClosed)
                    _traceServer.Close();
                _traceServer.Dispose();
            }
            catch (Exception e)
            {
                OnDisconnectError(e);
                return false;
            }
            Logger.Log("Trace:Disconnect(): Finished", LogEntryType.Information);
            return true;
        }

        public bool Reconnect()
        {
            if (!Disconnect())
                return false;

            Init();

            if (!Connect())
                return false;

            return true;
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                Disconnect();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DbContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
