using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class UpdateWarehouseSlotUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvWarehouseRepository _warehouseRepo;

        public UpdateWarehouseSlotUseCase(IUnitOfWork uow, IInvWarehouseRepository warehouseRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        public async Task<bool> SetLabelAsync(WarehouseSlotUpdateDto dto, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.UpdateLabelAndGradeAsync(dto, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.ClearLabelAsync(key, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
