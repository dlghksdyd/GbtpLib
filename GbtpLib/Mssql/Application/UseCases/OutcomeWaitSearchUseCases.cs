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
    /// Provides search for outcome-wait warehouse batteries with optional filters.
    /// </summary>
    public class OutcomeWaitSearchUseCases
    {
        private readonly ISlotQueryRepository _slots;
        public OutcomeWaitSearchUseCases(ISlotQueryRepository slots)
        {
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
        }

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
                return q.ToList();
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeWaitSearchUseCases.SearchAsync failed.", ex);
                throw;
            }
        }
    }
}
