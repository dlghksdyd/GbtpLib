using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class IncomeFlowUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;
        private readonly IInvWarehouseRepository _warehouseRepo;
        public IncomeFlowUseCases(IUnitOfWork uow, IStoredProcedureExecutor sp, IItfCmdDataRepository repo, IItfCmdDataQueries queries, IInvWarehouseRepository warehouseRepo)
        {
            _uow = uow; _sp = sp; _repo = repo; _queries = queries; _warehouseRepo = warehouseRepo;
        }

        public async Task<bool> ProcessAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new CommandPollingUseCase(_uow, _queries, _repo)
                    .WaitForAndAcknowledgeAsync(EIfCmd.AA3, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch
            {
                throw;
            }
        }
    }
}
