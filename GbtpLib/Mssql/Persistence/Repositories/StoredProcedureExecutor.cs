using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly IAppDbContext _db;
        public StoredProcedureExecutor(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<int> ExecuteAsync(string procedureName, IDictionary<string, object> parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Build parameterized EXEC statement for EF6 ExecuteSqlCommand
            var sql = $"EXEC {procedureName} {BuildPlaceholders(parameters)}";
            var args = BuildSqlParameters(parameters);
            return _db.ExecuteSqlCommandAsync(sql, cancellationToken, args);
        }

        private static string BuildPlaceholders(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return string.Empty;
            return string.Join(", ", System.Linq.Enumerable.Select(parameters, kv => kv.Key));
        }

        private static object[] BuildSqlParameters(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return new object[0];
            var list = new List<object>();
            foreach (var kv in parameters)
            {
                list.Add(new SqlParameter(kv.Key, kv.Value ?? DBNull.Value));
            }
            return list.ToArray();
        }
    }
}
