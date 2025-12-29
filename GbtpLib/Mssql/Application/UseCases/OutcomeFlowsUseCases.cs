using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Unified outcome flow use cases. Delegates to InterfaceFlowUseCases.
    /// Kept for backward compatibility.
    /// </summary>
    public class OutcomeFlowUseCases
    {
        private readonly InterfaceFlowUseCases _flows;

        public OutcomeFlowUseCases(IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries)
        {
            // warehouseRepo was not used in flow processing; kept for signature compatibility.
            _flows = new InterfaceFlowUseCases(sp, repo, queries);
        }

        /// <summary>
        /// Processes an outcome to loading step by acknowledging command AA3 for the given label.
        /// </summary>
        public Task<bool> ProcessOutcomeToLoadingAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try { return _flows.ProcessAa3Async(label, ct); }
            catch (Exception ex) { AppLog.Error("OutcomeFlowUseCases.ProcessOutcomeToLoadingAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Processes completion of a defect transfer to loading by acknowledging command EE8.
        /// </summary>
        public Task<bool> ProcessDefectToLoadingCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try { return _flows.ProcessEe8Async(label, ct); }
            catch (Exception ex) { AppLog.Error("OutcomeFlowUseCases.ProcessDefectToLoadingCompletedAsync failed.", ex); throw; }
        }
    }
}
