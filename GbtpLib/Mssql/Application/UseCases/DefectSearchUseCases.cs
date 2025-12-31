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
    /// Provides search for defect-warehouse batteries with optional filters.
    /// Falls back to in-memory filtering of slot results from ISlotQueryRepository.
    /// </summary>
    public class DefectSearchUseCases
    {
        private readonly ISlotQueryRepository _slots;
        public DefectSearchUseCases(ISlotQueryRepository slots)
        {
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
        }

        public async Task<IReadOnlyList<SlotInfoDto>> SearchAsync(
            string siteCode,
            string factoryCode,
            string defectWarehouseCode,
            string labelSubstring,
            string carMakeName,
            string carName,
            string batteryMakeName,
            string releaseYear,
            string batteryTypeName,
            DateTime? startCollectDate,
            DateTime? endCollectDate,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                // ISlotQueryRepository does not expose a generic method; use outcome-wait query for arbitrary warehouse
                var list = await _slots.GetOutcomeWaitSlotsAsync(siteCode, factoryCode, defectWarehouseCode, ct).ConfigureAwait(false);
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
                if (startCollectDate.HasValue)
                    q = q.Where(x => ParseDate(x.CollectDate) >= startCollectDate.Value);
                if (endCollectDate.HasValue)
                    q = q.Where(x => ParseDate(x.CollectDate) <= endCollectDate.Value);
                return q.ToList();
            }
            catch (Exception ex)
            {
                AppLog.Error("DefectSearchUseCases.SearchAsync failed.", ex);
                throw;
            }
        }

        private static DateTime ParseDate(string yyyymmdd)
        {
            if (string.IsNullOrWhiteSpace(yyyymmdd) || yyyymmdd.Length != 8) return DateTime.MinValue;
            int y = int.Parse(yyyymmdd.Substring(0, 4));
            int m = int.Parse(yyyymmdd.Substring(4, 2));
            int d = int.Parse(yyyymmdd.Substring(6, 2));
            return new DateTime(y, m, d);
        }
    }
}
