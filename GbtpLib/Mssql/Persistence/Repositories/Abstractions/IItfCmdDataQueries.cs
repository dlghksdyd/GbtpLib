using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IItfCmdDataQueries
    {
        Task<IReadOnlyList<ItfCmdDataEntity>> GetPendingAsync(string cmdCode, string data1, CancellationToken ct = default(CancellationToken));
        Task<IReadOnlyList<ItfCmdDataEntity>> GetPendingByCmdAsync(string cmdCode, CancellationToken ct = default(CancellationToken));
    }
}
