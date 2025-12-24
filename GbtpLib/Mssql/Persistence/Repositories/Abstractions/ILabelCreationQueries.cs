using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface ILabelCreationQueries
    {
        Task<IReadOnlyList<LabelCreationInfoDto>> GetLabelCreationInfosAsync(CancellationToken ct = default(CancellationToken));
    }
}
