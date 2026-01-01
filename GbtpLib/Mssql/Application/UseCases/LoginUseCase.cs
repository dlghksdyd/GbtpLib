using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Performs user login against MST_USER_INFO.
    /// <para>
    /// Return semantics: (<c>success</c>, <c>isAdmin</c>) where success indicates valid credentials and isAdmin is true when the user group is ADMN.
    /// Exceptions are propagated.
    /// </para>
    /// </summary>
    public class LoginUseCases
    {
        private readonly IMstUserInfoRepository _repo;

        public LoginUseCases(IMstUserInfoRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        public async Task<(bool success, bool isAdmin)> LoginAsync(string userId, string password, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"Login start user={userId}");
                var user = await _repo.GetByIdPasswordAsync(userId, password, ct).ConfigureAwait(false);
                if (user == null) { sw.Stop(); AppLog.Info($"Login done user={userId}, success=false, elapsedMs={sw.ElapsedMilliseconds}"); return (false, false); }
                var isAdmin = string.Equals(user.UserGroupCode, "ADMN", System.StringComparison.OrdinalIgnoreCase);
                sw.Stop();
                AppLog.Info($"Login done user={userId}, success=true, isAdmin={isAdmin}, elapsedMs={sw.ElapsedMilliseconds}");
                return (true, isAdmin);
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"Login error user={userId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
