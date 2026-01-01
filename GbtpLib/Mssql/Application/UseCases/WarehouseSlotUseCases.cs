using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

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
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.Get start site={site}, factory={factory}, wh={warehouse}"); var list = await _slotQueries.GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.Get done site={site}, factory={factory}, wh={warehouse}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.Get error site={site}, factory={factory}, wh={warehouse}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        // Back-compat (deprecated): Outcome-wait specific name
        [Obsolete("Use GetWarehouseSlotsAsync for both income/outcome contexts.")]
        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.GetOutcomeWait start site={site}, factory={factory}, wh={warehouse}"); var list = await GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.GetOutcomeWait done site={site}, factory={factory}, wh={warehouse}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.GetOutcomeWait error site={site}, factory={factory}, wh={warehouse}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.GetLoading start site={site}, factory={factory}, wh={warehouse}"); var list = await _slotQueries.GetWarehouseSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.GetLoading done site={site}, factory={factory}, wh={warehouse}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.GetLoading error site={site}, factory={factory}, wh={warehouse}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        // Commands (from UpdateWarehouseSlotUseCase)
        public async Task<bool> SetLabelAsync(WarehouseSlotUpdateDto dto, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.SetLabel start wh={dto?.WarehouseCode}, r={dto?.Row}, c={dto?.Col}, l={dto?.Level}"); var affected = await _warehouseRepo.UpdateLabelAndGradeAsync(dto, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.SetLabel done rows={affected}, elapsedMs={sw.ElapsedMilliseconds}"); return affected > 0; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.SetLabel error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        public async Task<bool> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.ClearLabel start wh={key?.WarehouseCode}, r={key?.Row}, c={key?.Col}, l={key?.Level}"); var affected = await _warehouseRepo.ClearLabelAsync(key, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.ClearLabel done rows={affected}, elapsedMs={sw.ElapsedMilliseconds}"); return affected > 0; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.ClearLabel error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Updates STORE_DIV for a slot. Consolidated here from WarehouseSlotCommandUseCases.
        /// </summary>
        public async Task<bool> SetStoreDivAsync(int row, int col, int lvl, string warehouseCode, string storeDiv, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"Slots.SetStoreDiv start wh={warehouseCode}, r={row}, c={col}, l={lvl}, div={storeDiv}"); var affected = await _warehouseRepo.UpdateStoreDivAsync(row, col, lvl, warehouseCode, storeDiv, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"Slots.SetStoreDiv done rows={affected}, elapsedMs={sw.ElapsedMilliseconds}"); return affected > 0; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"Slots.SetStoreDiv error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
    }
}
