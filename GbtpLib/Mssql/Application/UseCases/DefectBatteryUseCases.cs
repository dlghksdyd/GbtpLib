using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class DefectBatteryUseCases
    {
        private readonly IDefectBatteryQueries _queries;
        public DefectBatteryUseCases(IDefectBatteryQueries queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<DefectBatteryDto>> SearchAsync(
            string siteCode,
            string factoryCode,
            string warehouseCode,
            string labelSubstring,
            string startCollectionDateYmd,
            string endCollectionDateYmd,
            string carManufacture,
            string carModel,
            string batteryManufacture,
            string releaseYear,
            string batteryType,
            string selectedGrade,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.SearchAsync(
                    siteCode,
                    factoryCode,
                    warehouseCode,
                    labelSubstring,
                    startCollectionDateYmd,
                    endCollectionDateYmd,
                    carManufacture,
                    carModel,
                    batteryManufacture,
                    releaseYear,
                    batteryType,
                    selectedGrade,
                    ct).ConfigureAwait(false);
                return list ?? new List<DefectBatteryDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error("DefectBatteryUseCases.SearchAsync failed.", ex);
                throw;
            }
        }
    }
}
