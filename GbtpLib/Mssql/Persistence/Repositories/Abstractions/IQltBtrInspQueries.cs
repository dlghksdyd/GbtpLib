using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IQltBtrInspQueries
    {
        Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken));

        Task<IReadOnlyList<InoutInspectionInfoDto>> GetInoutInspectionInfosAsync(
            string siteCode,
            string factoryCode,
            string processCode,
            string machineCode,
            string inspKindGroupCode,
            string inspKindCode,
            string labelId,
            CancellationToken ct = default(CancellationToken));
    }
}
