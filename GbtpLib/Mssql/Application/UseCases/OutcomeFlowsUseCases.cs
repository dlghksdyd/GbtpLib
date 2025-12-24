using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
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

        public async Task<bool> ProcessOutcomeToLoadingAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var ok = await new CommandPollingUseCase(_uow, _queries, _repo)
                    .WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return ok;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<bool> ProcessDefectToLoadingCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var ok = await new CommandPollingUseCase(_uow, _queries, _repo)
                    .WaitForAndAcknowledgeAsync(EIfCmd.EE8, label, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return ok;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
