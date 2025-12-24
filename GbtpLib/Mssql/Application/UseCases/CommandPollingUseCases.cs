using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Polls interface command data for a specific command code and key, and acknowledges it when found.
    /// <para>
    /// This use case queries pending rows in ITF_CMD_DATA matching the provided <paramref name="cmdToWait"/> and <paramref name="data1"/>.
    /// If at least one row exists, it performs an acknowledge operation and returns whether any rows were affected.
    /// </para>
    /// <para>
    /// Return semantics:
    /// - <c>true</c>: Pending command entries existed and at least one was acknowledged successfully.
    /// - <c>false</c>: No pending entries were found for the given key; nothing was acknowledged.
    /// - Exceptions are propagated; callers should handle transient failures or retry policies as needed.
    /// </para>
    /// </summary>
    public class CommandPollingUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataQueries _queries;
        private readonly IItfCmdDataRepository _repo;
        public CommandPollingUseCase(IUnitOfWork uow, IItfCmdDataQueries queries, IItfCmdDataRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Waits by checking for pending interface rows and acknowledges them when present.
        /// </summary>
        /// <param name="cmdToWait">Command code to look up.</param>
        /// <param name="data1">Primary key value (DATA1) used as a filter.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <c>true</c> when an acknowledge affected &gt; 0 rows; otherwise <c>false</c> when no pending rows were found.
        /// </returns>
        public async Task<bool> WaitForAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
                if (list.Count == 0) { return false; }
                var affected = await _repo.AcknowledgeAsync(cmdToWait, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
