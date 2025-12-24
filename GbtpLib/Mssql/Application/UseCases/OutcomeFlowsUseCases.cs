using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Handles outcome-related flows driven by interface command polling and acknowledgment.
    /// <para>
    /// Each operation polls ITF_CMD_DATA for specific command codes tied to a label, and acknowledges if present.
    /// Return semantics:
    /// - <c>true</c>: Matching pending entry found and acknowledged (affected &gt; 0).
    /// - <c>false</c>: No matching pending entry found.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class OutcomeFlowUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvWarehouseRepository _warehouseRepo;
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;
        public OutcomeFlowUseCases(IUnitOfWork uow, IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries)
        {
            _uow = uow; _warehouseRepo = warehouseRepo; _sp = sp; _repo = repo; _queries = queries;
        }

        /// <summary>
        /// Processes an outcome to loading step by acknowledging command AA3 for the given label.
        /// </summary>
        /// <param name="label">Label identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if acknowledged; otherwise <c>false</c> when not found.</returns>
        public async Task<bool> ProcessOutcomeToLoadingAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new CommandPollingUseCase(_uow, _queries, _repo)
                    .WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.ProcessOutcomeToLoadingAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Processes completion of a defect transfer to loading by acknowledging command EE8.
        /// </summary>
        /// <param name="label">Label identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if acknowledged; otherwise <c>false</c> when not found.</returns>
        public async Task<bool> ProcessDefectToLoadingCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new CommandPollingUseCase(_uow, _queries, _repo)
                    .WaitForAndAcknowledgeAsync(EIfCmd.EE8, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.ProcessDefectToLoadingCompletedAsync failed.", ex);
                throw;
            }
        }
    }
}
