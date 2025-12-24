using System;

namespace GbtpLib.Mssql.Configuration
{
    internal static class DbConnectionSettings
    {
        private static string _connectionString;
        private static bool _initialized;

        public static string ConnectionString
        {
            get
            {
                if (!_initialized || string.IsNullOrWhiteSpace(_connectionString))
                    throw new InvalidOperationException("DB connection string has not been initialized. Call DbConnectionSettings.Initialize(connectionString) once at startup.");
                return _connectionString;
            }
        }

        public static void Initialize(string connectionString)
        {
            if (_initialized)
                throw new InvalidOperationException("DB connection string is already initialized and cannot be changed.");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is null or empty.", nameof(connectionString));

            _connectionString = connectionString;
            _initialized = true;
        }

        public static bool IsInitialized => _initialized;
    }
}