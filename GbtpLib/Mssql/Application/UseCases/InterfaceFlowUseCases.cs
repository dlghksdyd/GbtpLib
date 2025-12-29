using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Consolidates similar interface-command driven flow operations (income/outcome) into one class.
    /// Provides unified methods to wait for and acknowledge specific commands for a label.
    /// </summary>
    public class InterfaceFlowUseCases
    {
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;

        public InterfaceFlowUseCases(IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// Waits for and acknowledges command AA3 for the given label.
        /// </summary>
        public async Task<bool> ProcessAa3Async(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new InterfaceCommandUseCases(_repo, _queries, _sp)
                    .WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceFlowUseCases.ProcessAa3Async failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Waits for and acknowledges command EE8 for the given label.
        /// </summary>
        public async Task<bool> ProcessEe8Async(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new InterfaceCommandUseCases(_repo, _queries, _sp)
                    .WaitForAndAcknowledgeAsync(EIfCmd.EE8, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceFlowUseCases.ProcessEe8Async failed.", ex);
                throw;
            }
        }
    }
}
