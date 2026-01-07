using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IMstCodeQueries
    {
        Task<string> GetCodeAsync(string group, string codeName, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<string>> GetNamesAsync(string group, CancellationToken cancellationToken = default(CancellationToken));
    }
}
