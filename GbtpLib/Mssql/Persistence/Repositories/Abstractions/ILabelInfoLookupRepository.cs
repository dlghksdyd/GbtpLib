using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ILabelInfoLookupRepository
    {
        Task<LabelInfoDto> GetByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken));
    }
}
