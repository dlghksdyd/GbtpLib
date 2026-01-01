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
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetSites start"); var list = await _queries.GetSitesAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetSites done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetSites error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets factories for a given site.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetFactoriesAsync(string siteCode, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"GetFactories start site={siteCode}"); var list = await _queries.GetFactoriesAsync(siteCode, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetFactories done site={siteCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetFactories error site={siteCode}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets warehouses for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetWarehousesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"GetWarehouses start site={siteCode}, factory={factoryCode}"); var list = await _queries.GetWarehousesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetWarehouses done site={siteCode}, factory={factoryCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetWarehouses error site={siteCode}, factory={factoryCode}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets processes for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetProcessesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"GetProcesses start site={siteCode}, factory={factoryCode}"); var list = await _queries.GetProcessesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetProcesses done site={siteCode}, factory={factoryCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetProcesses error site={siteCode}, factory={factoryCode}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets machines for a site/factory.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetMachinesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"GetMachines start site={siteCode}, factory={factoryCode}"); var list = await _queries.GetMachinesAsync(siteCode, factoryCode, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetMachines done site={siteCode}, factory={factoryCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetMachines error site={siteCode}, factory={factoryCode}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets inspection kind groups.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindGroupsAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetInspKindGroups start"); var list = await _queries.GetInspKindGroupsAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetInspKindGroups done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetInspKindGroups error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }

        /// <summary>
        /// Gets inspection kinds within a group.
        /// </summary>
        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindsAsync(string inspKindGroupCode, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace($"GetInspKinds start group={inspKindGroupCode}"); var list = await _queries.GetInspKindsAsync(inspKindGroupCode, ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetInspKinds done group={inspKindGroupCode}, count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetInspKinds error group={inspKindGroupCode}, elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
    }
}
