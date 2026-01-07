using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    // Query-side: label info lookup only
    public class LabelQueries : ILabelInfoLookupRepository
    {
        private readonly IAppDbContext _db;
        public LabelQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<LabelInfoDto> GetByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var query = from b in _db.Set<MstBtrEntity>().AsNoTracking()
                        join t in _db.Set<MstBtrTypeEntity>().AsNoTracking() on b.BatteryTypeNo equals t.BatteryTypeNo
                        join cm in _db.Set<MstCarMakeEntity>().AsNoTracking() on t.CarMakeCode equals cm.CarMakeCode into cmJoin
                        from cm in cmJoin.DefaultIfEmpty()
                        join car in _db.Set<MstCarEntity>().AsNoTracking() on t.CarCode equals car.CarCode into carJoin
                        from car in carJoin.DefaultIfEmpty()
                        join bm in _db.Set<MstBtrMakeEntity>().AsNoTracking() on t.BatteryMakeCode equals bm.BatteryMakeCode into bmJoin
                        from bm in bmJoin.DefaultIfEmpty()
                        where b.LabelId == labelId
                        select new LabelInfoDto
                        {
                            Site = b.SiteCode,
                            CollectionDate = b.CollectDate,
                            ColtDat = b.CollectDate,
                            CarMakeNm = cm != null ? cm.CarMakeName : null,
                            CarNm = car != null ? car.CarName : null,
                            CarRelsYear = t.CarReleaseYear,
                            BtrMakeNm = bm != null ? bm.BatteryMakeName : null,
                            BtrTypeNm = t.BatteryTypeName,
                        };

            return await query.FirstOrDefaultAsync(ct).ConfigureAwait(false);
        }
    }
}
