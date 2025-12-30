using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

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
            try
            {
                return await _queries.GetInoutInspectionInfosAsync(siteCode, factoryCode, processCode, machineCode, inspKindGroupCode, inspKindCode, labelId, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppLog.Error("InoutInspectionUseCases.GetInspectionInfosAsync failed.", ex);
                throw;
            }
        }
    }
}
