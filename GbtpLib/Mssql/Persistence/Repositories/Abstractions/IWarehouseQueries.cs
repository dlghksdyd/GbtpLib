using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain.ReadModels;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IWarehouseQueries
    {
        Task<IReadOnlyList<WarehouseCodeNameDto>> GetWarehousesAsync(string siteCode, string factCode, string whType, CancellationToken ct = default(CancellationToken));
    }
}
