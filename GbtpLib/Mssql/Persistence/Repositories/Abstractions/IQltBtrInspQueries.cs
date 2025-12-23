using System.Threading;
using System.Threading.Tasks;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IQltBtrInspQueries
    {
        Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken));
    }
}
