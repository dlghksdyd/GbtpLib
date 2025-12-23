using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IStoredProcedureExecutor
    {
        Task<int> ExecuteAsync(string procedureName, IDictionary<string, object> parameters, CancellationToken cancellationToken = default(CancellationToken));
    }
}
