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
    /// Retrieves site/factory metadata for selection UIs.
    /// <para>
    /// Methods return read-only lists of CodeNameDto; empty lists indicate no data; exceptions are propagated.
    /// </para>
    /// </summary>
    public class MetadataUseCases
    {
        private readonly IMetadataQueries _queries;

        public MetadataUseCases(IMetadataQueries queries)
        {
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
                AppLog.Error($"MetadataUseCases.GetFactoriesAsync failed. site={siteCode}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets warehouses for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetWarehousesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetWarehousesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error($"MetadataUseCases.GetWarehousesAsync failed. site={siteCode}, factory={factoryCode}", ex); throw; }
        }

        /// <summary>
        /// Gets processes for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetProcessesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetProcessesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error($"MetadataUseCases.GetProcessesAsync failed. site={siteCode}, factory={factoryCode}", ex); throw; }
        }

        /// <summary>
        /// Gets machines for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetMachinesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetMachinesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error($"MetadataUseCases.GetMachinesAsync failed. site={siteCode}, factory={factoryCode}", ex); throw; }
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
            catch (Exception ex) { AppLog.Error($"MetadataUseCases.GetInspKindsAsync failed. group={inspKindGroupCode}", ex); throw; }
        }

        /// <summary>
        /// Gets all grade names.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetGradeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetGradeNamesAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetGradeNamesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets all car make names.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetCarMakeNamesAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetCarMakeNamesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets all car names.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetCarNamesAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetCarNamesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets all battery make names.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetBatteryMakeNamesAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetBatteryMakeNamesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets all battery type names.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetBatteryTypeNamesAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetBatteryTypeNamesAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets all release years.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetReleaseYearsAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetReleaseYearsAsync failed.", ex); throw; }
        }

        /// <summary>
        /// Gets battery types with year.
        /// </summary>
        public async Task<IReadOnlyList<BatteryTypeYearDto>> GetBatteryTypesWithYearAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _queries.GetBatteryTypesWithYearAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("MetadataUseCases.GetBatteryTypesWithYearAsync failed.", ex); throw; }
        }
    }
}
