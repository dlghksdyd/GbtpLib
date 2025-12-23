using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // Reference flow: delete label from MST_BTR, then clear matching INV_WAREHOUSE slots
    public class DeleteLabelFlowUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;
        private readonly IInvWarehouseRepository _whRepo;

        public DeleteLabelFlowUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo, IInvWarehouseRepository whRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
        }

        public async Task<bool> ExecuteAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var del = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                if (del < 1)
                {
                    await _uow.RollbackAsync(ct).ConfigureAwait(false);
                    return false;
                }

                await _whRepo.ClearLabelByLabelIdAsync(labelId, siteCode, factoryCode, warehouseCode, ct).ConfigureAwait(false);

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
