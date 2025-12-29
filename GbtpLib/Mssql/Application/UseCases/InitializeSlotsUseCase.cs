using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Initializes UI slot lists by querying outcome-wait and loading slot groups.
    /// <para>
    /// Return semantics: methods return read-only lists; empty lists indicate no data; exceptions are propagated.
    /// </para>
    /// </summary>
    public class InitializeSlotsUseCase
    {
        private readonly ISlotQueryRepository _repo;

        public InitializeSlotsUseCase(ISlotQueryRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Gets outcome-wait slots.
        /// </summary>
        public async Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _repo.GetOutcomeWaitSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeSlotsUseCase.GetOutcomeWaitAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets loading slots.
        /// </summary>
        public async Task<IReadOnlyList<SlotInfoDto>> GetLoadingAsync(string site, string fact, string wh, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _repo.GetLoadingSlotsAsync(site, fact, wh, ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("InitializeSlotsUseCase.GetLoadingAsync failed.", ex);
                throw;
            }
        }
    }
}
