using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class LoginUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstUserInfoRepository _repo;

        public LoginUseCase(IUnitOfWork uow, IMstUserInfoRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<(bool success, bool isAdmin)> LoginAsync(string userId, string password, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var user = await _repo.GetByIdPasswordAsync(userId, password, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                if (user == null) return (false, false);
                var isAdmin = string.Equals(user.UserGroupCode, "ADMN", StringComparison.OrdinalIgnoreCase);
                return (true, isAdmin);
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
