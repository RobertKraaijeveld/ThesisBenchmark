using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarking_Console_App.Configurations.Databases.DatabaseApis.SQL
{
    public abstract class AbstractSqlOperationModel
    {
        protected string ValueToString(object value)
        {
            var type = value.GetType();

            if (type == typeof(String)) return $"'{value}'";
            else return value.ToString();
        }
    }
}
