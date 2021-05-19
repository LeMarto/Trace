using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceBackend.Database.CommandFactory;
using TraceBackend.Utils;
using static TraceBackend.Utils.Logger;

namespace TraceBackend.Database
{
    public class DbContext : IDisposable
    {
        public delegate void DbContextError(Exception e);
        public event DbContextError OnConnectError;
        public event DbContextError OnDisconnectError;
        internal SqlConnection Connection { get; private set; }
        private SqlConnectionStringBuilder _csb;
        public string ServerName
        {
            get { return _csb.DataSource; }
            set { _csb.DataSource = value; }
        }
        public TabularQueryCommandFactory TabularQueryCommands { get; private set; }
        public DbContext()
        {
            string dbName = Properties.Settings.Default.Db_Name;
            _csb = new SqlConnectionStringBuilder()
            {
                ApplicationName = "Trace",
                InitialCatalog = dbName,
                IntegratedSecurity = true
            };
            Init();
        }
        private void Init()
        {
            Connection = new SqlConnection();
            TabularQueryCommands = new TabularQueryCommandFactory(this);
        }

        public bool Disconnect()
        {
            Logger.Log("DB: Disconnect(): Started", LogEntryType.Information);
            if (Connection.State == System.Data.ConnectionState.Closed)
                return true;
            try
            {
                Connection.Close();
            }
            catch (Exception e)
            {
                OnDisconnectError(e);
                return false;
            }
            Logger.Log("DB: Disconnect(): Finished", LogEntryType.Information);
            return true;
        }

        public bool Connect()
        {
            Logger.Log("DB: Connect(): Started.", LogEntryType.Information);
            if (Connection.State == System.Data.ConnectionState.Open)
                return true;

            try
            {
                Connection.ConnectionString = _csb.ToString();
                Connection.Open();
            }
            catch (Exception e)
            {
                OnConnectError(e);
                return false;
            }
            Logger.Log("DB: Connect(): Finished.", LogEntryType.Information);
            return true;
        }

        public bool Reconnect()
        {
            if (!Disconnect())
                return false;

            Init();

            if (!Reconnect())
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

                Connection.Close();
                Connection.Dispose();

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
