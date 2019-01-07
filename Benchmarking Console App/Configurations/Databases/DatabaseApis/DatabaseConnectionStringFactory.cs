using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Benchmarking_program.Configurations.Databases.DatabaseTypes;

namespace Benchmarking_program.Configurations.Databases.DatabaseApis
{
    public static class DatabaseConnectionStringFactory
    {
        public static readonly Dictionary<string, string> ConnectionStringPerDatabase;

        static DatabaseConnectionStringFactory()
        {
            string configFileName = GetConfigFileName();

            ConnectionStringPerDatabase = 
                File.ReadAllLines(configFileName) 
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

        public static bool IsConfigFileForScaledServersUsed()
        {
            var configFileToUse = GetConfigFileName();
            return configFileToUse.Contains("unscaled.config") == false;
        }

        public static string GetDatabaseConnectionString(EDatabaseType databaseType)
        {
            var databaseTypeStr = databaseType.ToString();

            if (ConnectionStringPerDatabase.ContainsKey(databaseTypeStr))
            {
                return ConnectionStringPerDatabase[databaseTypeStr];
            }
            else
            {
                throw new Exception("Database type does not have implementation yet");
            }
        }

        private static string GetConfigFileName()
        {
            return File.ReadAllLines("../../configFileToUse.config")
                       .First();
        }
    }
}
