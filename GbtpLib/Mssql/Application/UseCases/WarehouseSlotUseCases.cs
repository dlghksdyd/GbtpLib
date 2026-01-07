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
    /// Aggregates warehouse-related operations (initialize/query + assign/clear/update) and outcome flow searches.
    /// </summary>
    public class WarehouseUseCases
    {
        private readonly IWarehouseQueries _slotQueries;
        private readonly IWarehouseCommands _warehouseRepo;

        public WarehouseUseCases(IWarehouseQueries slotQueries, IWarehouseCommands warehouseRepo)
        {
            _slotQueries = slotQueries ?? throw new ArgumentNullException(nameof(slotQueries));
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        // Queries
        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _slotQueries.GetOutcomeWaitSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false);
                return list ?? Array.Empty<SlotInfoDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error($"WarehouseUseCases.GetOutcomeWaitAsync failed. site={site}, factory={factory}, warehouse={warehouse}", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string factory, string warehouse, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _slotQueries.GetLoadingSlotsAsync(site, factory, warehouse, ct).ConfigureAwait(false);
                return list ?? Array.Empty<SlotInfoDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error($"WarehouseUseCases.GetLoadingAsync failed. site={site}, factory={factory}, warehouse={warehouse}", ex);
                throw;
            }
        }

        // Mutations
        public async Task<bool> SetLabelAsync(WarehouseSlotUpdateDto dto, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.UpdateLabelAndGradeAsync(dto, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error($"WarehouseUseCases.SetLabelAsync failed. site={dto?.SiteCode}, factory={dto?.FactoryCode}, warehouse={dto?.WarehouseCode}, row={dto?.Row}, col={dto?.Col}, lvl={dto?.Level}, label={dto?.LabelId}", ex);
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
            catch (Exception ex)
            {
                AppLog.Error($"WarehouseUseCases.ClearLabelAsync failed. site={key?.SiteCode}, factory={key?.FactoryCode}, warehouse={key?.WarehouseCode}, row={key?.Row}, col={key?.Col}, lvl={key?.Level}", ex);
                throw;
            }
        }

        public async Task<bool> UpdateStoreDivAsync(WarehouseSlotKeyDto key, string storeDiv, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.UpdateStoreDivAsync(key, storeDiv, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error($"WarehouseUseCases.UpdateStoreDivAsync failed. site={key?.SiteCode}, factory={key?.FactoryCode}, warehouse={key?.WarehouseCode}, row={key?.Row}, col={key?.Col}, lvl={key?.Level}, storeDiv={storeDiv}", ex);
                throw;
            }
        }

        // Outcome flow search (merged from OutcomeFlowUseCases)
        public async Task<IReadOnlyList<GradeClassBatteryDbDto>> SearchGradeWarehouseBatteriesAsync(
            string siteCode,
            string factoryCode,
            string gradeWarehouseCode,
            string labelSubstring,
            string selectedGrade,
            DateTime startCollectionDate,
            DateTime endCollectionDate,
            string carManufacture,
            string carModel,
            string batteryManufacture,
            string releaseYear,
            string batteryType,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _slotQueries.SearchGradeWarehouseBatteriesAsync(
                    siteCode,
                    factoryCode,
                    gradeWarehouseCode,
                    labelSubstring,
                    selectedGrade,
                    startCollectionDate,
                    endCollectionDate,
                    carManufacture,
                    carModel,
                    batteryManufacture,
                    releaseYear,
                    batteryType,
                    ct).ConfigureAwait(false);
                return list ?? new List<GradeClassBatteryDbDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error("WarehouseUseCases.SearchGradeWarehouseBatteriesAsync failed.", ex);
                throw;
            }
        }
    }
}
