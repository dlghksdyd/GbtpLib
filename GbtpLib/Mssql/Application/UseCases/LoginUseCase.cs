using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

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
        private readonly IUnitOfWork _uow;
        private readonly IMstUserInfoRepository _repo;

        public LoginUseCase(IUnitOfWork uow, IMstUserInfoRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
            catch
            {
                throw;
            }
        }
    }
}
