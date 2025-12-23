namespace GbtpLib.Configuration
{
    public static class DbConnectionSettings
    {
        // Consumers can set this at app startup to configure EF connection.
        public static string ConnectionString { get; set; }
    }
}
