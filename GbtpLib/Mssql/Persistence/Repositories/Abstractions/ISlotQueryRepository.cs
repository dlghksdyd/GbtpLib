using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain.ReadModels;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ISlotQueryRepository
    {
        Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));
        Task<IReadOnlyList<SlotInfoDto>> GetLoadingSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));
    }
}
