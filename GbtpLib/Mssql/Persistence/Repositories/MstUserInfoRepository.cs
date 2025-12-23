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
    public class MstUserInfoRepository : IMstUserInfoRepository
    {
        private readonly IAppDbContext _db;
        public MstUserInfoRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<MstUserInfoEntity> GetByIdPasswordAsync(string userId, string password, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();
            return _db.Set<MstUserInfoEntity>()
                .Where(x => x.UserId == userId && x.Password == password)
                .FirstOrDefaultAsync(ct);
        }
    }
}
