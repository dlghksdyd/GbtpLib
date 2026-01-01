using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides inspection info lookups for income/outcome views.
    /// </summary>
    public class InoutInspectionUseCases
    {
        private readonly IQltBtrInspQueries _queries;
        public InoutInspectionUseCases(IQltBtrInspQueries queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<InoutInspectionInfoDto>> GetInspectionInfosAsync(
            string siteCode,
            string factoryCode,
            string processCode,
            string machineCode,
            string inspKindGroupCode,
            string inspKindCode,
            string labelId,
            CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetInspectionInfos start site={siteCode}, factory={factoryCode}");
                var list = await _queries.GetInoutInspectionInfosAsync(siteCode, factoryCode, processCode, machineCode, inspKindGroupCode, inspKindCode, labelId, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetInspectionInfos done site={siteCode}, factory={factoryCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}");
                return list;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetInspectionInfos error site={siteCode}, factory={factoryCode}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
