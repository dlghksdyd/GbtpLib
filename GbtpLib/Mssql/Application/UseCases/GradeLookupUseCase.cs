using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Looks up the latest inspection grade for a given label.
    /// <para>
    /// Return semantics: returns the latest grade string (may be null/empty if none exists); exceptions are propagated.
    /// </para>
    /// </summary>
    public class GradeLookupUseCases
    {
        private readonly IQltBtrInspQueries _queries;

        public GradeLookupUseCases(IQltBtrInspQueries queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// Gets the latest grade value for a label.
        /// </summary>
        public async Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetLatestGrade start label={labelId}");
                var grade = await _queries.GetLatestGradeAsync(labelId, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetLatestGrade done label={labelId}, elapsedMs={sw.ElapsedMilliseconds}");
                return grade;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetLatestGrade error label={labelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
