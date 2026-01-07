using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    // Query-side for master/code/user/type lookups
    public class MasterDataQueries : IMstBtrTypeRepository, IMstCodeRepository, IMstUserInfoRepository
    {
        private readonly IAppDbContext _db;
        public MasterDataQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // IMstBtrTypeRepository
        public Task<MstBtrTypeEntity> GetByNoAsync(int batteryTypeNo, CancellationToken ct = default(CancellationToken))
        {
            return _db.Set<MstBtrTypeEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.BatteryTypeNo == batteryTypeNo, ct);
        }

        // IMstCodeRepository
        public async Task<string> GetCodeAsync(string group, string codeName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var code = await _db.Set<MstCodeEntity>().AsNoTracking()
                .Where(x => x.CodeGroup == group && x.CodeName == codeName)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            return code ?? string.Empty;
        }

        public async Task<IReadOnlyList<string>> GetNamesAsync(string group, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var names = await _db.Set<MstCodeEntity>().AsNoTracking()
                .Where(x => x.CodeGroup == group && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CodeName)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return names;
        }

        // IMstUserInfoRepository
        public Task<MstUserInfoEntity> GetByIdPasswordAsync(string userId, string password, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();
            return _db.Set<MstUserInfoEntity>().AsNoTracking()
                .Where(x => x.UserId == userId && x.Password == password)
                .FirstOrDefaultAsync(ct);
        }
    }
}
