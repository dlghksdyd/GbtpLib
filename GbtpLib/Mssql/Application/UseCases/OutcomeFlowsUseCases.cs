using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // High-level flows corresponding to Outcome system actions
    public class OutcomeFlowUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvWarehouseRepository _warehouseRepo;
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _cmdRepo;
        private readonly IItfCmdDataQueries _cmdQuery;

        public OutcomeFlowUseCases(IUnitOfWork uow, IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor sp, IItfCmdDataRepository cmdRepo, IItfCmdDataQueries cmdQuery)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
            _cmdRepo = cmdRepo ?? throw new ArgumentNullException(nameof(cmdRepo));
            _cmdQuery = cmdQuery ?? throw new ArgumentNullException(nameof(cmdQuery));
        }

        public async Task<bool> MoveOutcomeWaitToLoadingAsync(string label, string site, string fact, string fromWh, int fromRow, int fromCol, int fromLvl, string toWh, int toRow, int toCol, int toLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            // Request via SP then update warehouse slots accordingly (two locations)
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var ok = await new RequestTransferUseCase(_uow, _sp)
                    .RequestAcceptAsync(label, fromRow, fromCol, fromLvl, reqSys, ct).ConfigureAwait(false);
                if (!ok) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                // Wait for EE8/AA3-like completion signal (simplified: single-shot check)
                var polled = await new CommandPollingUseCase(_uow, _cmdQuery, _cmdRepo)
                    .WaitForAndAcknowledgeAsync(IfCmd.AA3, label, ct).ConfigureAwait(false);
                if (!polled) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                // Update: clear from outcome-wait slot
                await _warehouseRepo.ClearLabelAsync(new Domain.WarehouseSlotKeyDto
                {
                    SiteCode = site, FactoryCode = fact, WarehouseCode = fromWh,
                    Row = fromRow.ToString(), Col = fromCol.ToString(), Level = fromLvl.ToString(),
                }, ct).ConfigureAwait(false);

                // Update: set to loading slot
                await _warehouseRepo.UpdateLabelAndGradeAsync(new Domain.WarehouseSlotUpdateDto
                {
                    SiteCode = site, FactoryCode = fact, WarehouseCode = toWh,
                    Row = toRow.ToString(), Col = toCol.ToString(), Level = toLvl.ToString(),
                    LabelId = label, LoadGrade = null,
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

        public async Task<bool> MoveDefectToLoadingAsync(string label, string site, string fact, string defectWh, int defectRow, int defectCol, int defectLvl, string loadingWh, int loadingRow, int loadingCol, int loadingLvl, string reqSys, string grade = null, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var ok = await new RequestTransferUseCase(_uow, _sp)
                    .RequestDefectToLoadingAsync(label, defectRow, defectCol, defectLvl, loadingRow, loadingCol, loadingLvl, reqSys, ct).ConfigureAwait(false);
                if (!ok) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                // Wait for EE8 completion
                var polled = await new CommandPollingUseCase(_uow, _cmdQuery, _cmdRepo)
                    .WaitForAndAcknowledgeAsync(IfCmd.EE8, label, ct).ConfigureAwait(false);
                if (!polled) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }

                // Clear defect slot
                await _warehouseRepo.ClearLabelAsync(new Domain.WarehouseSlotKeyDto
                {
                    SiteCode = site, FactoryCode = fact, WarehouseCode = defectWh,
                    Row = defectRow.ToString(), Col = defectCol.ToString(), Level = defectLvl.ToString(),
                }, ct).ConfigureAwait(false);

                // Set loading slot with optional grade
                await _warehouseRepo.UpdateLabelAndGradeAsync(new Domain.WarehouseSlotUpdateDto
                {
                    SiteCode = site, FactoryCode = fact, WarehouseCode = loadingWh,
                    Row = loadingRow.ToString(), Col = loadingCol.ToString(), Level = loadingLvl.ToString(),
                    LabelId = label, LoadGrade = grade,
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
