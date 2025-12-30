using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class QltBtrInoutInspRepository : IQltBtrInoutInspRepository
    {
        private readonly IAppDbContext _db;
        public QltBtrInoutInspRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> InsertAsync(QltBtrInoutInspEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            _db.Set<QltBtrInoutInspEntity>().Add(entity);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
