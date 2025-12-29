using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Performs user login against MST_USER_INFO.
    /// <para>
    /// Return semantics: (<c>success</c>, <c>isAdmin</c>) where success indicates valid credentials and isAdmin is true when the user group is ADMN.
    /// Exceptions are propagated.
    /// </para>
    /// </summary>
    public class LoginUseCase
    {
        private readonly IMstUserInfoRepository _repo;

        public LoginUseCase(IMstUserInfoRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        public async Task<(bool success, bool isAdmin)> LoginAsync(string userId, string password, CancellationToken ct = default)
        {
            try
            {
                var user = await _repo.GetByIdPasswordAsync(userId, password, ct).ConfigureAwait(false);
                if (user == null) return (false, false);
                var isAdmin = string.Equals(user.UserGroupCode, "ADMN", System.StringComparison.OrdinalIgnoreCase);
                return (true, isAdmin);
            }
            catch (Exception ex)
            {
                AppLog.Error("LoginUseCase.LoginAsync failed.", ex);
                throw;
            }
        }
    }
}
