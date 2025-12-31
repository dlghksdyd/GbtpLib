using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using System;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ISlotQueryRepository
    {
        /// <summary>
        /// General warehouse slot query by warehouse code. Use this instead of context-specific names.
        /// </summary>
        Task<IReadOnlyList<SlotInfoDto>> GetWarehouseSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));

        [Obsolete("Use GetWarehouseSlotsAsync instead.")]
        Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));

        [Obsolete("Use GetWarehouseSlotsAsync instead.")]
        Task<IReadOnlyList<SlotInfoDto>> GetLoadingSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));
    }
}
