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
            // Build EXEC with named assignments in fixed stored procedure parameter order
            var sql = $"EXEC {procedureName} {BuildNamedAssignments()}";
            var args = BuildOrderedSqlParameters(parameters);
            return _db.ExecuteSqlCommandAsync(sql, cancellationToken, args);
        }

        // Fixed order and named assignments to avoid positional binding issues
        private static string BuildNamedAssignments()
        {
            var orderedKeys = new[]
            {
                "@IN_CMD_CD",
                "@IN_DATA1",
                "@IN_DATA2",
                "@IN_DATA3",
                "@IN_DATA4",
                "@IN_DATA5",
                "@IN_DATA6",
                "@IN_DATA7",
                "@IN_DATA8",
                "@IN_DATA9",
                "@IN_DATA10",
                "@IN_REQ_SYS",
            };
            var parts = new List<string>(orderedKeys.Length);
            foreach (var key in orderedKeys)
            {
                parts.Add($"{key}={key}");
            }
            return string.Join(", ", parts);
        }

        // Create SqlParameters in fixed order, defaulting missing values to string.Empty
        private static object[] BuildOrderedSqlParameters(IDictionary<string, object> parameters)
        {
            var orderedKeys = new[]
            {
                "@IN_CMD_CD",
                "@IN_DATA1",
                "@IN_DATA2",
                "@IN_DATA3",
                "@IN_DATA4",
                "@IN_DATA5",
                "@IN_DATA6",
                "@IN_DATA7",
                "@IN_DATA8",
                "@IN_DATA9",
                "@IN_DATA10",
                "@IN_REQ_SYS",
            };

            var list = new List<object>(orderedKeys.Length);
            foreach (var key in orderedKeys)
            {
                object value = string.Empty; // default to empty string for missing params
                if (parameters != null && parameters.TryGetValue(key, out var v))
                {
                    value = v ?? string.Empty;
                }
                list.Add(new SqlParameter(key, value));
            }
            return list.ToArray();
        }
    }
}
