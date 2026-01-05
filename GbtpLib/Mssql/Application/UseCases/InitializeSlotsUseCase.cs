using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class InitializeSlotsUseCase
    {
        private readonly ISlotQueryRepository _repo;

        public InitializeSlotsUseCase(ISlotQueryRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _repo.GetOutcomeWaitSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                return list ?? Array.Empty<SlotInfoDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error($"InitializeSlotsUseCase.GetOutcomeWaitAsync failed. site={site}, fact={fact}, wh={wh}", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _repo.GetLoadingSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                return list ?? Array.Empty<SlotInfoDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error($"InitializeSlotsUseCase.GetLoadingAsync failed. site={site}, fact={fact}, wh={wh}", ex);
                throw;
            }
        }
    }
}
