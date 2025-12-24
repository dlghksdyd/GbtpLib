using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides metadata required for label creation forms.
    /// <para>
    /// Methods return read-only lists; exceptions are propagated.
    /// </para>
    /// </summary>
    public class LabelCreationMetadataUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly ILabelCreationQueries _queries;

        public LabelCreationMetadataUseCase(IUnitOfWork uow, ILabelCreationQueries queries)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// Gets label creation info list.
        /// </summary>
        public async Task<IReadOnlyList<LabelCreationInfoDto>> GetAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetLabelCreationInfosAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch
            {
                throw;
            }
        }
    }
}
