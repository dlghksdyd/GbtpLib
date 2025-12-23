using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // High-level flows corresponding to Income system actions (accept/reject requests to outcome wait slots)
    public class IncomeFlowUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _cmdRepo;
        private readonly IItfCmdDataQueries _cmdQuery;
        private readonly IInvWarehouseRepository _warehouseRepo;

        public IncomeFlowUseCases(IUnitOfWork uow, IStoredProcedureExecutor sp, IItfCmdDataRepository cmdRepo, IItfCmdDataQueries cmdQuery, IInvWarehouseRepository warehouseRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
            _cmdRepo = cmdRepo ?? throw new ArgumentNullException(nameof(cmdRepo));
            _cmdQuery = cmdQuery ?? throw new ArgumentNullException(nameof(cmdQuery));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        public async Task<bool> RequestAndAssignOutcomeWaitAsync(string label, string site, string fact, string outcomeWh, int row, int col, int lvl, string grade, string reqSys, bool accept, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var request = new RequestTransferUseCase(_uow, _sp);
                bool ok;
                if (accept)
                {
                    ok = await request.RequestAcceptAsync(label, row, col, lvl, reqSys, ct).ConfigureAwait(false);
                }
                else
                {
                    ok = await request.RequestRejectAsync(label, row, col, lvl, reqSys, ct).ConfigureAwait(false);
                }
                if (!ok) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                var polled = await new CommandPollingUseCase(_uow, _cmdQuery, _cmdRepo)
                    .WaitForAndAcknowledgeAsync(IfCmd.AA3, label, ct).ConfigureAwait(false);
                if (!polled) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                // Assign/update the outcome wait slot
                await _warehouseRepo.UpdateLabelAndGradeAsync(new Domain.WarehouseSlotUpdateDto
                {
                    SiteCode = site,
                    FactoryCode = fact,
                    WarehouseCode = outcomeWh,
                    Row = row.ToString(),
                    Col = col.ToString(),
                    Level = lvl.ToString(),
                    LabelId = label,
                    LoadGrade = grade,
                }, ct).ConfigureAwait(false);

                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return true;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
