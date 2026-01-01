using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides dropdown lists for defect/outcome filter UIs.
    /// <para>
    /// Methods return read-only lists; empty lists indicate no data; exceptions are propagated.
    /// </para>
    /// </summary>
    public class FilterMetadataUseCases
    {
        private readonly IMetadataQueries _queries;
        public FilterMetadataUseCases(IMetadataQueries queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetCarMakeNames start"); var list = await _queries.GetCarMakeNamesAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetCarMakeNames done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetCarMakeNames error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetCarNames start"); var list = await _queries.GetCarNamesAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetCarNames done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetCarNames error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetBatteryMakeNames start"); var list = await _queries.GetBatteryMakeNamesAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetBatteryMakeNames done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetBatteryMakeNames error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetBatteryTypeNames start"); var list = await _queries.GetBatteryTypeNamesAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetBatteryTypeNames done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetBatteryTypeNames error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try { AppLog.Trace("GetReleaseYears start"); var list = await _queries.GetReleaseYearsAsync(ct).ConfigureAwait(false); sw.Stop(); AppLog.Info($"GetReleaseYears done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}"); return list; }
            catch (Exception ex) { sw.Stop(); AppLog.Error($"GetReleaseYears error elapsedMs={sw.ElapsedMilliseconds}", ex); throw; }
        }
    }
}
