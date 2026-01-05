using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using GbtpLib.Mssql.Domain; // Moved DTO here

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Outcome-related consolidated use cases.
    /// Provides search operations backed by queries.
    /// </summary>
    public class OutcomeFlowUseCases
    {
        private readonly ISlotQueryRepository _slotQueries;

        public OutcomeFlowUseCases(ISlotQueryRepository slotQueries)
        {
            _slotQueries = slotQueries ?? throw new ArgumentNullException(nameof(slotQueries));
        }

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
                AppLog.Error("OutcomeFlowUseCases.SearchGradeWarehouseBatteriesAsync failed.", ex);
                throw;
            }
        }
    }
}
