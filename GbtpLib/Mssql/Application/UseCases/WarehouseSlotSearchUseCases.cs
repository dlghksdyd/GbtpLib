using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

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
            try
            {
                var list = await _slots.GetOutcomeWaitSlotsAsync(siteCode, factoryCode, warehouseCode, ct).ConfigureAwait(false);
                IEnumerable<SlotInfoDto> q = list;

                if (!string.IsNullOrWhiteSpace(labelSubstring))
                    q = q.Where(x => (x.LabelId ?? string.Empty).IndexOf(labelSubstring, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrWhiteSpace(carMakeName))
                    q = q.Where(x => string.Equals(x.CarMakeName, carMakeName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(carName))
                    q = q.Where(x => string.Equals(x.CarName, carName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(batteryMakeName))
                    q = q.Where(x => string.Equals(x.BatteryMakeName, batteryMakeName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(releaseYear))
                    q = q.Where(x => string.Equals(x.CarReleaseYear, releaseYear, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(batteryTypeName))
                    q = q.Where(x => string.Equals(x.BatteryTypeName, batteryTypeName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(grade))
                    q = q.Where(x => string.Equals(x.InspectGrade, grade, StringComparison.OrdinalIgnoreCase));
                if (startCollectDate.HasValue)
                    q = q.Where(x => ParseDate(x.CollectDate) >= startCollectDate.Value);
                if (endCollectDate.HasValue)
                    q = q.Where(x => ParseDate(x.CollectDate) <= endCollectDate.Value);

                return q.ToList();
            }
            catch (Exception ex)
            {
                AppLog.Error("WarehouseSlotSearchUseCases.SearchAsync failed.", ex);
                throw;
            }
        }

        private static DateTime ParseDate(string yyyymmdd)
        {
            if (string.IsNullOrWhiteSpace(yyyymmdd) || yyyymmdd.Length != 8) return DateTime.MinValue;
            int y, m, d;
            if (!int.TryParse(yyyymmdd.Substring(0, 4), out y)) return DateTime.MinValue;
            if (!int.TryParse(yyyymmdd.Substring(4, 2), out m)) return DateTime.MinValue;
            if (!int.TryParse(yyyymmdd.Substring(6, 2), out d)) return DateTime.MinValue;
            return new DateTime(y, m, d);
        }
    }
}
