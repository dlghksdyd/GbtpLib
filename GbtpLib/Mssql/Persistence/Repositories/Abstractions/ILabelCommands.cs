using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ILabelCommands
    {
        Task<int> InsertAsync(MstBtrEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> DeleteAsync(string labelId, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> UpdatePrintYnAsync(string labelId, string printYn, CancellationToken cancellationToken = default(CancellationToken));
    }
}
