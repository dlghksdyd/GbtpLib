using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class QltBtrInspQueries : IQltBtrInspQueries
    {
        private readonly IAppDbContext _db;
        public QltBtrInspQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var q = _db.Set<QltBtrInspEntity>().AsNoTracking()
                .Where(x => x.LabelId == labelId && x.BatteryDiagStatus == "Y")
                .OrderByDescending(x => x.InspectSeq)
                .Select(x => x.InspectGrade);

            var grade = await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
            return grade;
        }
    }
}
