using System;
using System.Collections.Generic;
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
    public class SlotQueryRepository : ISlotQueryRepository
    {
        private readonly IAppDbContext _db;
        public SlotQueryRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            return GetSlotsAsync(siteCode, factCode, whCode, ct);
        }

        public Task<IReadOnlyList<SlotInfoDto>> GetLoadingSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            return GetSlotsAsync(siteCode, factCode, whCode, ct);
        }

        private async Task<IReadOnlyList<SlotInfoDto>> GetSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            // Latest inspect seq per label where diag status = 'Y'
            var latest = _db.Set<QltBtrInspEntity>().AsNoTracking()
                .Where(q => q.BatteryDiagStatus == "Y")
                .GroupBy(q => q.LabelId)
                .Select(g => new { LabelId = g.Key, InspectSeq = g.Max(x => x.InspectSeq) });

            var query = from inv in _db.Set<InvWarehouseEntity>().AsNoTracking()
                        // left join latest inspection key on label
                        join li in latest on inv.LabelId equals li.LabelId into liJoin
                        from li in liJoin.DefaultIfEmpty()
                        // left join actual inspection row matching latest key
                        join inspAll in _db.Set<QltBtrInspEntity>().AsNoTracking() on new { inv.LabelId, InspectSeq = (string)li.InspectSeq } equals new { inspAll.LabelId, inspAll.InspectSeq } into inspJoin
                        from insp in inspJoin.DefaultIfEmpty()
                        // other left joins
                        join site in _db.Set<MstSiteEntity>().AsNoTracking() on inv.SiteCode equals site.SiteCode into siteJoin
                        from site in siteJoin.DefaultIfEmpty()
                        join btr in _db.Set<MstBtrEntity>().AsNoTracking() on inv.LabelId equals btr.LabelId into btrJoin
                        from btr in btrJoin.DefaultIfEmpty()
                        join btrType in _db.Set<MstBtrTypeEntity>().AsNoTracking() on btr.BatteryTypeNo equals btrType.BatteryTypeNo into btrTypeJoin
                        from btrType in btrTypeJoin.DefaultIfEmpty()
                        join carMake in _db.Set<MstCarMakeEntity>().AsNoTracking() on btrType.CarMakeCode equals carMake.CarMakeCode into carMakeJoin
                        from carMake in carMakeJoin.DefaultIfEmpty()
                        join car in _db.Set<MstCarEntity>().AsNoTracking() on btrType.CarCode equals car.CarCode into carJoin
                        from car in carJoin.DefaultIfEmpty()
                        join btrMake in _db.Set<MstBtrMakeEntity>().AsNoTracking() on btrType.BatteryMakeCode equals btrMake.BatteryMakeCode into btrMakeJoin
                        from btrMake in btrMakeJoin.DefaultIfEmpty()
                        where inv.SiteCode == siteCode
                           && inv.FactoryCode == factCode
                           && inv.WarehouseCode == whCode
                           && (btr == null || btr.UseYn == "Y")
                        select new { inv, insp, site, btr, btrType, carMake, car, btrMake };

            // Project only EF-translatable members in SQL, then convert types in memory
            var rawList = await query
                .Select(x => new
                {
                    Row = x.inv.Row,
                    Col = x.inv.Col,
                    Lvl = x.inv.Level,
                    LabelId = x.inv.LabelId,
                    InspectGrade = x.insp != null ? x.insp.InspectGrade : null,
                    SiteName = x.site != null ? x.site.SiteName : null,
                    CollectDate = x.btr != null ? x.btr.CollectDate : null,
                    CollectReason = x.btr != null ? x.btr.CollectReason : null,
                    PackModuleCode = x.btr != null ? x.btr.PackModuleCode : null,
                    BatteryTypeName = x.btrType != null ? x.btrType.BatteryTypeName : null,
                    CarReleaseYear = x.btrType != null ? x.btrType.CarReleaseYear : null,
                    CarMakeName = x.carMake != null ? x.carMake.CarMakeName : null,
                    CarName = x.car != null ? x.car.CarName : null,
                    BatteryMakeName = x.btrMake != null ? x.btrMake.BatteryMakeName : null,
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var list = rawList.Select(r => new SlotInfoDto
            {
                Row = SafeParseInt(r.Row),
                Col = SafeParseInt(r.Col),
                Lvl = SafeParseInt(r.Lvl),
                LabelId = r.LabelId,
                InspectGrade = r.InspectGrade,
                SiteName = r.SiteName,
                CollectDate = r.CollectDate,
                CollectReason = r.CollectReason,
                PackModuleCode = r.PackModuleCode,
                BatteryTypeName = r.BatteryTypeName,
                CarReleaseYear = r.CarReleaseYear,
                CarMakeName = r.CarMakeName,
                CarName = r.CarName,
                BatteryMakeName = r.BatteryMakeName,
            }).ToList();

            return list;
        }

        private static int SafeParseInt(string s)
        {
            int v; return int.TryParse(s, out v) ? v : 0;
        }
    }
}
