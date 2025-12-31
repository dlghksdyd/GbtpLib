using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates warehouse slot-related operations (initialize/query + assign/clear label/grade) into a single class.
    /// </summary>
    public class WarehouseSlotUseCases
    {
        private readonly ISlotQueryRepository _slotQueries;
        private readonly IInvWarehouseRepository _warehouseRepo;

        public WarehouseSlotUseCases(ISlotQueryRepository slotQueries, IInvWarehouseRepository warehouseRepo)
        {
            _slotQueries = slotQueries ?? throw new ArgumentNullException(nameof(slotQueries));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        // Generic query usable by both Income and Outcome contexts
        public async Task<IReadOnlyList<SlotInfoDto>> GetWarehouseSlotsAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            try { return await _slotQueries.GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("WarehouseSlotUseCases.GetWarehouseSlotsAsync failed.", ex); throw; }
        }

        // Back-compat (deprecated): Outcome-wait specific name
        [Obsolete("Use GetWarehouseSlotsAsync for both income/outcome contexts.")]
        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            return await GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            try { return await _slotQueries.GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("WarehouseSlotUseCases.GetLoadingAsync failed.", ex); throw; }
        }

        // Commands (from UpdateWarehouseSlotUseCase)
        public async Task<bool> SetLabelAsync(WarehouseSlotUpdateDto dto, CancellationToken ct = default(CancellationToken))
        {
            try { var affected = await _warehouseRepo.UpdateLabelAndGradeAsync(dto, ct).ConfigureAwait(false); return affected > 0; }
            catch (Exception ex) { AppLog.Error("WarehouseSlotUseCases.SetLabelAsync failed.", ex); throw; }
        }

        public async Task<bool> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken ct = default(CancellationToken))
        {
            try { var affected = await _warehouseRepo.ClearLabelAsync(key, ct).ConfigureAwait(false); return affected > 0; }
            catch (Exception ex) { AppLog.Error("WarehouseSlotUseCases.ClearLabelAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Updates STORE_DIV for a slot. Consolidated here from WarehouseSlotCommandUseCases.
        /// </summary>
        public async Task<bool> SetStoreDivAsync(int row, int col, int lvl, string warehouseCode, string storeDiv, CancellationToken ct = default(CancellationToken))
        {
            try { var affected = await _warehouseRepo.UpdateStoreDivAsync(row, col, lvl, warehouseCode, storeDiv, ct).ConfigureAwait(false); return affected > 0; }
            catch (Exception ex) { AppLog.Error("WarehouseSlotUseCases.SetStoreDivAsync failed.", ex); throw; }
        }
    }
}
