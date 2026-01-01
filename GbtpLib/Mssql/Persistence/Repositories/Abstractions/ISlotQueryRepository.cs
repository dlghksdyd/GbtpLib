using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ISlotQueryRepository
    {
        /// <summary>
        /// General warehouse slot query by warehouse code. Use this instead of context-specific names.
        /// </summary>
        Task<IReadOnlyList<SlotInfoDto>> GetWarehouseSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));

        /// <summary>
        /// Server-side filtered search (can be optimized to push filters into EF query when needed).
        /// </summary>
        Task<IReadOnlyList<SlotInfoDto>> SearchWarehouseSlotsAsync(WarehouseSlotSearchFilterDto filter, CancellationToken ct = default(CancellationToken));
    }
}
