using System;
using GbtpLib.Mssql.Persistence.Abstractions;

namespace GbtpLib.Mssql.Persistence.Db
{
    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly string _connectionString;
        public AppDbContextFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IAppDbContext Create()
        {
            var ef = new AppEfDbContext(_connectionString);
            return new AppDbContext(ef);
        }
    }
}
