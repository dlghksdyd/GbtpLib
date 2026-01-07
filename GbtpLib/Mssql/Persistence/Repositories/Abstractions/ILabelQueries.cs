using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ILabelQueries
    {
        Task<IReadOnlyList<LabelCreationInfoDto>> GetLabelCreationInfosAsync(CancellationToken ct = default(CancellationToken));
        Task<LabelInfoDto> GetByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken));
        Task<int> GetNextVersionAsync(string collectDate, CancellationToken cancellationToken = default(CancellationToken));
    }
}
