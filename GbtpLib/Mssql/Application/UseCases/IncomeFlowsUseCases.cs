using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Handles income-related flows by polling and acknowledging interface commands.
    /// <para>
    /// Return semantics:
    /// - <c>true</c>: Matching pending entry found and acknowledged (affected &gt; 0).
    /// - <c>false</c>: No matching pending entry found.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class IncomeFlowUseCases
    {
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;
        private readonly IInvWarehouseRepository _warehouseRepo;
        public IncomeFlowUseCases(IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries, IInvWarehouseRepository warehouseRepo)
        {
            _sp = sp; _repo = repo; _queries = queries; _warehouseRepo = warehouseRepo;
        }

        /// <summary>
        /// Processes a single income flow step by acknowledging AA3 for the given label.
        /// </summary>
        public async Task<bool> ProcessAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new InterfaceCommandUseCases(_repo, _queries, _sp)
                    .WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("IncomeFlowUseCases.ProcessAsync failed.", ex);
                throw;
            }
        }
    }
}
