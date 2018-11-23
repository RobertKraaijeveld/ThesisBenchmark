using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarking_Console_App.Configurations.Databases
{
    public static class UtilityFunctions
    {
        public static string FlattenQueries(string[] queries)
        {
            string ret = "";
            for (int i = 0; i < queries.Length; i++)
            {
                ret += $"{queries[i]}";
            }
            return ret;
        }
    }
}
