using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Retrieves site/factory metadata for selection UIs.
    /// <para>
    /// Methods return read-only lists of CodeNameDto; empty lists indicate no data; exceptions are propagated.
    /// </para>
    /// </summary>
    public class MetadataUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IMetadataQueries _queries;

        public MetadataUseCases(IUnitOfWork uow, IMetadataQueries queries)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// Gets all sites.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetSitesAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetSitesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("MetadataUseCases.GetSitesAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets factories for a given site.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetFactoriesAsync(string siteCode, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetFactoriesAsync(siteCode, ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("MetadataUseCases.GetFactoriesAsync failed.", ex);
                throw;
            }
        }
    }
}
