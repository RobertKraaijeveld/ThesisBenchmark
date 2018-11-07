namespace Benchmarking_program.Configurations.Databases.DatabaseTypes
{
    /**
    This class holds the constants for:
        - The key of the database type KV pair in appsettings.json
        - The key of the database connection string KV pair in appsettings.json
        
    */
    public static class DatabaseSettingsConstants
    {
        public static readonly string DATABASE_TYPE_SETTING_NAME = "databaseType";
        public static readonly string DATABASE_CONNECTIONSTRING_SETTING_NAME = "connectionString";
        public static readonly string DATABASE_SCALED_BOOL = "scaled";
    }
}
