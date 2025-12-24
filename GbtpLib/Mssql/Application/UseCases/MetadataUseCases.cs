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

        /// <summary>
        /// Gets warehouses for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetWarehousesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetWarehousesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetWarehousesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets processes for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetProcessesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetProcessesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetProcessesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets machines for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetMachinesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetMachinesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetMachinesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets inspection kind groups.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindGroupsAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetInspKindGroupsAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetInspKindGroupsAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets inspection kinds within a group.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindsAsync(string inspKindGroupCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetInspKindsAsync(inspKindGroupCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetInspKindsAsync failed.", ex); throw; }
        }
    }
}
