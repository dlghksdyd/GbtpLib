using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides accessors to common code values by code group.
    /// <para>
    /// Methods return the resolved code string; exceptions are propagated.
    /// </para>
    /// </summary>
    public class GetCodeUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstCodeRepository _repo;

        public GetCodeUseCase(IUnitOfWork uow, IMstCodeRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Gets a battery state code from STS003.
        /// </summary>
        public async Task<string> GetBatteryStateCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            // Reference: STS003
            try
            {
                var code = await _repo.GetCodeAsync("STS003", codeName, ct).ConfigureAwait(false);
                return code;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets an income division code from INV001.
        /// </summary>
        public async Task<string> GetIncomeDivisionCodeAsync(string codeName, CancellationToken ct = default(CancellationToken))
        {
            // Reference: INV001
            try
            {
                var code = await _repo.GetCodeAsync("INV001", codeName, ct).ConfigureAwait(false);
                return code;
            }
            catch
            {
                throw;
            }
        }
    }
}
