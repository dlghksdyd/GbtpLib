using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IDefectBatteryQueries
    {
        Task<IReadOnlyList<DefectBatteryDto>> SearchAsync(string site, string fact, string defectWh,
            string labelLike,
            string startCollectionDate,
            string endCollectionDate,
            string carMakeName,
            string carName,
            string batteryMakeName,
            string releaseYear,
            string batteryTypeName,
            string selectedGrade,
            CancellationToken ct = default(CancellationToken));
    }
}
