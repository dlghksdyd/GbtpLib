using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class CreateLabelAndAssignSlotUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;
        private readonly IInvWarehouseRepository _whRepo;

        public CreateLabelAndAssignSlotUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo, IInvWarehouseRepository whRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
        }

        public async Task<bool> ExecuteAsync(MstBtrEntity label, WarehouseSlotUpdateDto slot, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ins = await _btrRepo.InsertAsync(label, ct).ConfigureAwait(false);
                if (ins < 1)
                {
                    return false;
                }

                var upd = await _whRepo.UpdateLabelAndGradeAsync(slot, ct).ConfigureAwait(false);
                if (upd < 1)
                {
                    // rollback label creation if slot update failed (logical compensation; outer tx will rollback too)
                    await _btrRepo.DeleteAsync(label.LabelId, ct).ConfigureAwait(false);
                    return false;
                }

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
