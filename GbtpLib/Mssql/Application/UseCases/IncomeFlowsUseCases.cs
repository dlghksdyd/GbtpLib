using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Unified income flow use cases. Delegates to InterfaceFlowUseCases.
    /// Kept for backward compatibility.
    /// </summary>
    public class IncomeFlowUseCases
    {
        private readonly InterfaceFlowUseCases _flows;

        public IncomeFlowUseCases(IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries, IInvWarehouseRepository warehouseRepo)
        {
            // warehouseRepo was unused in previous implementation; kept in signature for compatibility.
            _flows = new InterfaceFlowUseCases(sp, repo, queries);
        }

        /// <summary>
        /// Processes a single income flow step by acknowledging AA3 for the given label.
        /// </summary>
        public Task<bool> ProcessAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try { return _flows.ProcessAa3Async(label, ct); }
            catch (Exception ex) { AppLog.Error("IncomeFlowUseCases.ProcessAsync failed.", ex); throw; }
        }
    }
}
