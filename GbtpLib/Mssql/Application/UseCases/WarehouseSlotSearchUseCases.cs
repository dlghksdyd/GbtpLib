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
    /// Consolidated warehouse-slot search use cases for outcome-wait/defect warehouses with common filters.
    /// </summary>
    public class WarehouseSlotSearchUseCases
    {
        private readonly ISlotQueryRepository _slots;
        public WarehouseSlotSearchUseCases(ISlotQueryRepository slots)
        {
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
        }

        /// <summary>
        /// Searches slots in a given warehouse with unified optional filters.
        /// </summary>
        public async Task<IReadOnlyList<SlotInfoDto>> SearchAsync(
            string siteCode,
            string factoryCode,
            string warehouseCode,
            string labelSubstring,
            string carMakeName,
            string carName,
            string batteryMakeName,
            string releaseYear,
            string batteryTypeName,
            string grade,
            DateTime? startCollectDate,
            DateTime? endCollectDate,
            CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Slots.Search start site={siteCode}, factory={factoryCode}, wh={warehouseCode}");
                var filter = new WarehouseSlotSearchFilterDto
                {
                    SiteCode = siteCode,
                    FactoryCode = factoryCode,
                    WarehouseCode = warehouseCode,
                    LabelSubstring = labelSubstring,
                    CarMakeName = carMakeName,
                    CarName = carName,
                    BatteryMakeName = batteryMakeName,
                    ReleaseYear = releaseYear,
                    BatteryTypeName = batteryTypeName,
                    Grade = grade,
                    StartCollectDate = startCollectDate,
                    EndCollectDate = endCollectDate,
                };

                var list = await _slots.SearchWarehouseSlotsAsync(filter, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"Slots.Search done site={siteCode}, factory={factoryCode}, wh={warehouseCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}");
                return list;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Slots.Search error site={siteCode}, factory={factoryCode}, wh={warehouseCode}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
