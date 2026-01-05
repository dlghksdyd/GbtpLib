using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ISlotQueryRepository
    {
        Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));
        Task<IReadOnlyList<SlotInfoDto>> GetLoadingSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));

        // A Query: Search grade warehouse batteries with filters
        Task<IReadOnlyList<GradeClassBatteryDbDto>> SearchGradeWarehouseBatteriesAsync(
            string siteCode,
            string factCode,
            string gradeWarehouseCode,
            string labelSubstring,
            string selectedGrade,
            System.DateTime startCollectionDate,
            System.DateTime endCollectionDate,
            string carManufacture,
            string carModel,
            string batteryManufacture,
            string releaseYear,
            string batteryType,
            CancellationToken ct = default(CancellationToken));
    }
}
