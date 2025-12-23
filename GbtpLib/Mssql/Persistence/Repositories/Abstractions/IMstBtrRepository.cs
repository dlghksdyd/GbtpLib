using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IMstBtrRepository
    {
        Task<int> InsertAsync(MstBtrEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> DeleteAsync(string labelId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
