using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class GetCodeUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstCodeRepository _repo;

        public GetCodeUseCase(IUnitOfWork uow, IMstCodeRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<string> GetBatteryStateCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            // Reference: STS003
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var code = await _repo.GetCodeAsync("STS003", codeName, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return code;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<string> GetIncomeDivisionCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            // Reference: INV001
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var code = await _repo.GetCodeAsync("INV001", codeName, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return code;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
