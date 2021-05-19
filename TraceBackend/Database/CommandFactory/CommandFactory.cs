using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceBackend.Database.CommandFactory
{
    public abstract class CommandFactory : DbCommand
    {
        public CommandFactory(DbContext dbContext) : base(dbContext)
        {
        }
        public abstract SqlCommand GetSelectCommand();
        public abstract SqlCommand GetInsertCommand();
        public abstract SqlCommand GetUpdateCommand();
        public abstract SqlCommand GetDeleteCommand();
    }
}
