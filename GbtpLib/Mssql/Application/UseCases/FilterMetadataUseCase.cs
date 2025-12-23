using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // Provides dropdown lists for defect/outcome filter UIs
    public class FilterMetadataUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMetadataQueries _queries;
        public FilterMetadataUseCase(IUnitOfWork uow, IMetadataQueries queries)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try { var list = await _queries.GetCarMakeNamesAsync(ct).ConfigureAwait(false); await _uow.CommitAsync(ct).ConfigureAwait(false); return list; }
            catch { await _uow.RollbackAsync(ct).ConfigureAwait(false); throw; }
        }
        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try { var list = await _queries.GetCarNamesAsync(ct).ConfigureAwait(false); await _uow.CommitAsync(ct).ConfigureAwait(false); return list; }
            catch { await _uow.RollbackAsync(ct).ConfigureAwait(false); throw; }
        }
        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try { var list = await _queries.GetBatteryMakeNamesAsync(ct).ConfigureAwait(false); await _uow.CommitAsync(ct).ConfigureAwait(false); return list; }
            catch { await _uow.RollbackAsync(ct).ConfigureAwait(false); throw; }
        }
        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try { var list = await _queries.GetBatteryTypeNamesAsync(ct).ConfigureAwait(false); await _uow.CommitAsync(ct).ConfigureAwait(false); return list; }
            catch { await _uow.RollbackAsync(ct).ConfigureAwait(false); throw; }
        }
        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try { var list = await _queries.GetReleaseYearsAsync(ct).ConfigureAwait(false); await _uow.CommitAsync(ct).ConfigureAwait(false); return list; }
            catch { await _uow.RollbackAsync(ct).ConfigureAwait(false); throw; }
        }
    }
}
