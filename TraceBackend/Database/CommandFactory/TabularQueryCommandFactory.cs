using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend.Database.CommandFactory
{
    public class TabularQueryCommandFactory : CommandFactory
    {
        public TabularQueryCommandFactory(DbContext dbContext) : base(dbContext)
        {
        }

        public override SqlCommand GetDeleteCommand()
        {
            throw new NotImplementedException();
        }

        public override SqlCommand GetInsertCommand()
        {
            string schemaName = Properties.Settings.Default.Schema;
            string spName = Properties.Settings.Default.Sp_Name;
            string commandString = String.Format("[{0}].[{1}]", schemaName, spName);
            SqlCommand sqlCommand = GetCommand(commandString);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            SqlParameter eventClass = new SqlParameter()
            {
                ParameterName = "@event_class",
                DbType = DbType.String,
                Size = 255,
                Direction = ParameterDirection.Input
            };

            SqlParameter eventSubClass = new SqlParameter()
            {
                ParameterName = "@event_sub_class",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Input
            };

            SqlParameter ntUserName = new SqlParameter()
            {
                ParameterName = "@nt_user_name",
                DbType = DbType.String,
                Size = 255,
                Direction = ParameterDirection.Input
            };

            SqlParameter applicationName = new SqlParameter()
            {
                ParameterName = "@application_name",
                DbType = DbType.String,
                Size = 255,
                Direction = ParameterDirection.Input
            };

            SqlParameter startTime = new SqlParameter()
            {
                ParameterName = "@start_time",
                DbType = DbType.DateTime2,
                Direction = ParameterDirection.Input
            };

            SqlParameter duration = new SqlParameter()
            {
                ParameterName = "@duration",
                DbType = DbType.Int64,
                Direction = ParameterDirection.Input
            };

            SqlParameter databaseName = new SqlParameter()
            {
                ParameterName = "@database_name",
                DbType = DbType.String,
                Size = 255,
                Direction = ParameterDirection.Input
            };

            SqlParameter rC = new SqlParameter()
            {
                ParameterName = "@rc",
                DbType = DbType.Int32,
                Direction = ParameterDirection.ReturnValue
            };
            
            sqlCommand.Parameters.Add(eventClass);
            sqlCommand.Parameters.Add(eventSubClass);
            sqlCommand.Parameters.Add(databaseName);
            sqlCommand.Parameters.Add(ntUserName);
            sqlCommand.Parameters.Add(applicationName);
            sqlCommand.Parameters.Add(startTime);
            sqlCommand.Parameters.Add(duration);
            sqlCommand.Parameters.Add(rC);
            
            return sqlCommand;
        }

        public override SqlCommand GetSelectCommand()
        {
            throw new NotImplementedException();
        }

        public override SqlCommand GetUpdateCommand()
        {
            throw new NotImplementedException();
        }
    }
}
