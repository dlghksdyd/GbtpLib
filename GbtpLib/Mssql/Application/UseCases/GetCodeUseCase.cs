using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides accessors to common code values by code group.
    /// <para>
    /// Methods return the resolved code string; exceptions are propagated.
    /// </para>
    /// </summary>
    public class GetCodeUseCases
    {
        private readonly IMstCodeRepository _repo;

        public GetCodeUseCases(IMstCodeRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Gets all code names for a given group.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetNamesByGroupAsync(string group, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetNames start group={group}");
                var result = await _repo.GetNamesAsync(group, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetNames done group={group}, count={(result?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}");
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetNames error group={group}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        /// <summary>
        /// Resolves a code by group and code name.
        /// </summary>
        public async Task<string> GetCodeByGroupAsync(string group, string codeName, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetCode start group={group}, name={codeName}");
                var code = await _repo.GetCodeAsync(group, codeName, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetCode done group={group}, name={codeName}, elapsedMs={sw.ElapsedMilliseconds}");
                return code;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetCode error group={group}, name={codeName}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets a battery state code from STS003.
        /// </summary>
        public async Task<string> GetBatteryStateCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetBatteryStateCode start name={codeName}");
                var code = await _repo.GetCodeAsync("STS003", codeName, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetBatteryStateCode done name={codeName}, elapsedMs={sw.ElapsedMilliseconds}");
                return code;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetBatteryStateCode error name={codeName}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets an income division code from INV001.
        /// </summary>
        public async Task<string> GetIncomeDivisionCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetIncomeDivisionCode start name={codeName}");
                var code = await _repo.GetCodeAsync("INV001", codeName, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetIncomeDivisionCode done name={codeName}, elapsedMs={sw.ElapsedMilliseconds}");
                return code;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetIncomeDivisionCode error name={codeName}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
