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
    public class LabelCommands : ILabelCommands
    {
        private readonly IAppDbContext _db;
        public LabelCommands(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // Commands
        public async Task<int> InsertAsync(MstBtrEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            _db.Set<MstBtrEntity>().Add(entity);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> DeleteAsync(string labelId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await _db.Set<MstBtrEntity>()
                .Where(x => x.LabelId == labelId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (item == null) return 0;
            _db.Set<MstBtrEntity>().Remove(item);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> UpdatePrintYnAsync(string labelId, string printYn, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = await _db.Set<MstBtrEntity>()
                .Where(x => x.LabelId == labelId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            if (item == null) return 0;
            item.PrintYn = printYn;
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
