using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.DatabaseApis.SQL;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis
{
    public class DatabaseConnectionStringFactory
    {
        private Dictionary<string, string> connectionStringPerDatabase;

        public DatabaseConnectionStringFactory()
        {
            // Parsing appsettings.json to list of KV pairs
            connectionStringPerDatabase = 
                File.ReadAllLines("db.config")
                    .ToDictionary(key =>
                    {
                        return key.Split(':')[0];
                    },
                    val =>
                    {
                        var split = val.Split(':');
                        if (split.Length > 2)
                        {
                            // not just taking split[1] because string after first ':' might contain other ':'s,
                            // for example "connectionString: mongodb://1.2.3.4". We DO want those other ':'.
                            string restOfString = "";
                            for (int i = 1; i < split.Length; i++)
                            {
                                split[i] = split[i].TrimStart(' ');
                                restOfString += $"{split[i]}:";
                            }

                            restOfString = restOfString.Remove(restOfString.Length - 1, 1); // removing redundant ':' at end
                            return restOfString;
                        }
                        else
                        {
                            return split[1];
                        }
                    });
        }


        public string GetDatabaseConnectionString(EDatabaseType databaseType)
        {
            var databaseTypeStr = databaseType.ToString();

            if (connectionStringPerDatabase.ContainsKey(databaseTypeStr))
            {
                return connectionStringPerDatabase[databaseTypeStr];
            }
            else
            {
                throw new Exception("Database type does not have implementation yet");
            }
        }
    }
}
