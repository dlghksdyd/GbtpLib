using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain.ReadModels;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class MetadataUseCases
    {
        private readonly IUnitOfWork _uow;
        private readonly IMetadataQueries _queries;

        public MetadataUseCases(IUnitOfWork uow, IMetadataQueries queries)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetSitesAsync(CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await _queries.GetSitesAsync(ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetFactoriesAsync(string siteCode, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await _queries.GetFactoriesAsync(siteCode, ct).ConfigureAwait(false);
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
