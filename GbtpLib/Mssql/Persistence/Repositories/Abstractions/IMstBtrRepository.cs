using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IMstBtrRepository
    {
        Task<int> InsertAsync(MstBtrEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> DeleteAsync(string labelId, CancellationToken cancellationToken = default(CancellationToken));
        // Returns next Version (MAX(VER) + 1) for the given collect date (COLT_DAT). If none, returns 1.
        Task<int> GetNextVersionAsync(string collectDate, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> UpdatePrintYnAsync(string labelId, string printYn, CancellationToken cancellationToken = default(CancellationToken));
    }
}
