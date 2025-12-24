using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IWarehouseLayoutQueries
    {
        Task<IReadOnlyList<WarehouseSlotLayoutDto>> GetLayoutAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken));
    }
}
