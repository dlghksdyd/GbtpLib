using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class MstBtrTypeRepository : IMstBtrTypeRepository
    {
        private readonly IAppDbContext _db;
        public MstBtrTypeRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<MstBtrTypeEntity> GetByNoAsync(int batteryTypeNo, CancellationToken ct = default(CancellationToken))
        {
            return _db.Set<MstBtrTypeEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.BatteryTypeNo == batteryTypeNo, ct);
        }
    }
}
