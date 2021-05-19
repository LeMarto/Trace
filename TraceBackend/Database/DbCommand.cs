using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend.Database
{
    public class DbCommand
    {
        private DbContext _dbContext;
        private SqlConnection Connection { get { return _dbContext.Connection; } }
        public DbCommand(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected SqlCommand GetCommand(string commandString)
        {
            SqlCommand cmd = Connection.CreateCommand();

            cmd.CommandText = commandString;

            return cmd;
        }
    }
}
