using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IMstBtrTypeQueries
    {
        Task<MstBtrTypeEntity> GetByNoAsync(int batteryTypeNo, CancellationToken ct = default(CancellationToken));
    }
}
