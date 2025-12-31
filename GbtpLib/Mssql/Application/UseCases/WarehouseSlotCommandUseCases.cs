using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Warehouse slot attribute commands (e.g., StoreDiv update) not covered by label/grade operations.
    /// </summary>
    public class WarehouseSlotCommandUseCases
    {
        private readonly IInvWarehouseRepository _warehouseRepo;

        public WarehouseSlotCommandUseCases(IInvWarehouseRepository warehouseRepo)
        {
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        /// <summary>
        /// Updates STORE_DIV for a slot.
        /// Note: Repository does not support this yet; returns true to keep UI flow. Implement in repo as needed.
        /// </summary>
        public async Task<bool> SetStoreDivAsync(int row, int col, int lvl, string warehouseCode, string storeDiv, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.UpdateStoreDivAsync(row, col, lvl, warehouseCode, storeDiv, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("WarehouseSlotCommandUseCases.SetStoreDivAsync failed.", ex);
                throw;
            }
        }
    }
}
