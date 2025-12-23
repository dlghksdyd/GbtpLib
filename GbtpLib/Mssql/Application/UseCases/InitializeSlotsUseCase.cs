using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain.ReadModels;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class InitializeSlotsUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly ISlotQueryRepository _repo;

        public InitializeSlotsUseCase(IUnitOfWork uow, ISlotQueryRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await _repo.GetOutcomeWaitSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await _repo.GetLoadingSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
