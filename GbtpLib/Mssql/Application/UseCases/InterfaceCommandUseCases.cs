using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates interface command-related operations: enqueue, acknowledge, polling.
    /// Stored procedure calls have been centralized into StoredProcCommandUseCases.
    /// </summary>
    public class InterfaceCommandUseCases
    {
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;

        public InterfaceCommandUseCases(
            IItfCmdDataRepository repo,
            IItfCmdDataQueries queries,
            IStoredProcedureExecutor _ /* obsolete: kept for backward compat constructor overload */)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        // Repository-based operations
        public async Task<bool> EnqueueAsync(EIfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.EnqueueAsync(cmd, data1, data2, data3, data4, requestSystem, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.EnqueueAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.AcknowledgeAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> WaitForAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
                if (list.Count == 0) { return false; }
                var affected = await _repo.AcknowledgeAsync(cmdToWait, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.WaitForAndAcknowledgeAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Waits for any row with the given command and IF_FLAG pending, then acknowledges it (no DATA1 constraint).
        /// </summary>
        public async Task<bool> WaitForAndAcknowledgeByCmdAsync(EIfCmd cmdToWait, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), null, ct).ConfigureAwait(false);
                if (list.Count == 0) return false;
                var first = list[0];
                var affected = await _repo.AcknowledgeAsync(cmdToWait, first.Data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.WaitForAndAcknowledgeByCmdAsync failed.", ex);
                throw;
            }
        }
    }
}
