using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IQltBtrInoutInspRepository
    {
        Task<int> InsertAsync(QltBtrInoutInspEntity entity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
