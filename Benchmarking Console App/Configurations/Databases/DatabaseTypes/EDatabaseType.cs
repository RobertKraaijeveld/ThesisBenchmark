namespace Benchmarking_program.Configurations.Databases.DatabaseTypes
{

    /** 
        This enum contains the allowed values of the database type in appsettings.json
    */
    public enum EDatabaseType
    {
        MySQL,
        MySQLWithDapper,
        PostgreSQL,
        Perst,
        Redis,
        MongoDB,
        Cassandra
    }
}