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
    // Command-side: label CRUD only
    public class LabelRepository : IMstBtrRepository
    {
        private readonly IAppDbContext _db;
        public LabelRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> InsertAsync(MstBtrEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            _db.Set<MstBtrEntity>().Add(entity);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> DeleteAsync(string labelId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await _db.Set<MstBtrEntity>().FirstOrDefaultAsync(x => x.LabelId == labelId, cancellationToken).ConfigureAwait(false);
            if (item == null) return 0;
            _db.Set<MstBtrEntity>().Remove(item);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> GetNextVersionAsync(string collectDate, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = _db.Set<MstBtrEntity>().AsNoTracking().Where(x => x.CollectDate == collectDate);
            var maxVer = await query.MaxAsync(x => (int?)x.Version, cancellationToken).ConfigureAwait(false);
            if (!maxVer.HasValue || maxVer.Value < 1)
                return 1;
            return maxVer.Value + 1;
        }

        public async Task<int> UpdatePrintYnAsync(string labelId, string printYn, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await _db.Set<MstBtrEntity>().FirstOrDefaultAsync(x => x.LabelId == labelId, cancellationToken).ConfigureAwait(false);
            if (item == null) return 0;
            item.PrintYn = printYn;
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
