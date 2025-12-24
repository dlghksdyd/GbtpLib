using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Looks up the latest inspection grade for a given label.
    /// <para>
    /// Return semantics: returns the latest grade string (may be null/empty if none exists); exceptions are propagated.
    /// </para>
    /// </summary>
    public class GradeLookupUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IQltBtrInspQueries _queries;

        public GradeLookupUseCase(IUnitOfWork uow, IQltBtrInspQueries queries)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// Gets the latest grade value for a label.
        /// </summary>
        public async Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var grade = await _queries.GetLatestGradeAsync(labelId, ct).ConfigureAwait(false);
                return grade;
            }
            catch
            {
                throw;
            }
        }
    }
}
