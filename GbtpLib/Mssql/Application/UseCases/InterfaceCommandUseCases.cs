using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates interface command-related operations: enqueue, acknowledge, polling.
    /// Stored procedure calls have been centralized into StoredProcCommandUseCases.
    /// Also provides simple flow helpers that wait for and acknowledge specific commands.
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
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Enqueue start cmd={cmd}, reqSys={requestSystem}");
                var affected = await _repo.EnqueueAsync(cmd, data1, data2, data3, data4, requestSystem, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"Enqueue done cmd={cmd}, reqSys={requestSystem}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Enqueue error cmd={cmd}, reqSys={requestSystem}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        public async Task<bool> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Ack start cmd={cmd}");
                var affected = await _repo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"Ack done cmd={cmd}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Ack error cmd={cmd}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        public async Task<bool> WaitForAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Wait+Ack start cmd={cmdToWait}");
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
                if (list.Count == 0) { sw.Stop(); AppLog.Info($"Wait+Ack none cmd={cmdToWait}, elapsedMs={sw.ElapsedMilliseconds}"); return false; }
                var affected = await _repo.AcknowledgeAsync(cmdToWait, data1, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"Wait+Ack done cmd={cmdToWait}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Wait+Ack error cmd={cmdToWait}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        /// <summary>
        /// Waits for any row with the given command and IF_FLAG pending, then acknowledges it (no DATA1 constraint).
        /// </summary>
        public async Task<bool> WaitForAndAcknowledgeByCmdAsync(EIfCmd cmdToWait, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Wait+AckAny start cmd={cmdToWait}");
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), null, ct).ConfigureAwait(false);
                if (list.Count == 0) { sw.Stop(); AppLog.Info($"Wait+AckAny none cmd={cmdToWait}, elapsedMs={sw.ElapsedMilliseconds}"); return false; }
                var first = list[0];
                var affected = await _repo.AcknowledgeAsync(cmdToWait, first.Data1, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"Wait+AckAny done cmd={cmdToWait}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Wait+AckAny error cmd={cmdToWait}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        // =====================
        // Simple flow helpers (merged from InterfaceFlowUseCases)
        // =====================
        public Task<bool> ProcessAa3Async(string label, CancellationToken ct = default(CancellationToken))
        {
            return WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct);
        }

        public Task<bool> ProcessEe8Async(string label, CancellationToken ct = default(CancellationToken))
        {
            return WaitForAndAcknowledgeAsync(EIfCmd.EE8, label, ct);
        }
    }
}
