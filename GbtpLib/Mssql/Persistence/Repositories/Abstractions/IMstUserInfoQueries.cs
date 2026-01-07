using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IMstUserInfoQueries
    {
        Task<MstUserInfoEntity> GetByIdPasswordAsync(string userId, string password, CancellationToken ct = default(CancellationToken));
    }
}
