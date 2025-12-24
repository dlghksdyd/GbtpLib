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
    public class DefectBatteryQueries : IDefectBatteryQueries
    {
        private readonly IAppDbContext _db;
        public DefectBatteryQueries(IAppDbContext db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public async Task<IReadOnlyList<DefectBatteryDto>> SearchAsync(
            string site, string fact, string defectWh,
            string labelLike,
            string startCollectionDate,
            string endCollectionDate,
            string carMakeName,
            string carName,
            string batteryMakeName,
            string releaseYear,
            string batteryTypeName,
            string selectedGrade,
            CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var latest = _db.Set<QltBtrInspEntity>().AsNoTracking()
                .Where(q => q.BatteryDiagStatus == "Y" && q.InspKindCode == "DIAG")
                .GroupBy(q => q.LabelId)
                .Select(g => new { LabelId = g.Key, InspectSeq = g.Max(x => x.InspectSeq) });

            var baseQuery = from inv in _db.Set<InvWarehouseEntity>().AsNoTracking()
                            join siteTbl in _db.Set<MstSiteEntity>().AsNoTracking() on inv.SiteCode equals siteTbl.SiteCode into siteJoin
                            from siteTbl in siteJoin.DefaultIfEmpty()
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
                            // latest inspection join
                            join li in latest on inv.LabelId equals li.LabelId into liJoin
                            from li in liJoin.DefaultIfEmpty()
                            join inspAll in _db.Set<QltBtrInspEntity>().AsNoTracking() on new { inv.LabelId, InspectSeq = (string)li.InspectSeq } equals new { inspAll.LabelId, inspAll.InspectSeq } into inspJoin
                            from insp in inspJoin.DefaultIfEmpty()
                            where inv.SiteCode == site && inv.FactoryCode == fact && inv.WarehouseCode == defectWh
                               && (btr == null || btr.UseYn == "Y")
                            select new { inv, siteTbl, btr, btrType, carMake, car, btrMake, insp };

            if (!string.IsNullOrEmpty(labelLike)) baseQuery = baseQuery.Where(x => x.inv.LabelId.Contains(labelLike));
            if (!string.IsNullOrEmpty(startCollectionDate)) baseQuery = baseQuery.Where(x => x.btr.CollectDate.CompareTo(startCollectionDate) >= 0);
            if (!string.IsNullOrEmpty(endCollectionDate)) baseQuery = baseQuery.Where(x => x.btr.CollectDate.CompareTo(endCollectionDate) <= 0);
            if (!string.IsNullOrEmpty(carMakeName)) baseQuery = baseQuery.Where(x => x.carMake.CarMakeName == carMakeName);
            if (!string.IsNullOrEmpty(carName)) baseQuery = baseQuery.Where(x => x.car.CarName == carName);
            if (!string.IsNullOrEmpty(batteryMakeName)) baseQuery = baseQuery.Where(x => x.btrMake.BatteryMakeName == batteryMakeName);
            if (!string.IsNullOrEmpty(releaseYear)) baseQuery = baseQuery.Where(x => x.btrType.CarReleaseYear == releaseYear);
            if (!string.IsNullOrEmpty(batteryTypeName)) baseQuery = baseQuery.Where(x => x.btrType.BatteryTypeName == batteryTypeName);

            var list = await baseQuery
                .Select(x => new DefectBatteryDto
                {
                    Row = SafeParseInt(x.inv.Row),
                    Col = SafeParseInt(x.inv.Col),
                    Lvl = SafeParseInt(x.inv.Level),
                    LabelId = x.inv.LabelId,
                    SiteName = x.siteTbl != null ? x.siteTbl.SiteName : null,
                    CollectDate = x.btr != null ? x.btr.CollectDate : null,
                    CollectReason = x.btr != null ? x.btr.CollectReason : null,
                    DigInspFlag = x.btr != null ? x.btr.DigInspFlag : null,
                    PackModuleCode = x.btr != null ? x.btr.PackModuleCode : null,
                    BatteryTypeName = x.btrType != null ? x.btrType.BatteryTypeName : null,
                    CarReleaseYear = x.btrType != null ? x.btrType.CarReleaseYear : null,
                    CarMakeName = x.carMake != null ? x.carMake.CarMakeName : null,
                    CarName = x.car != null ? x.car.CarName : null,
                    BatteryMakeName = x.btrMake != null ? x.btrMake.BatteryMakeName : null,
                    Grade = x.insp != null ? x.insp.InspectGrade : null,
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(selectedGrade))
            {
                list = list.Where(x => x.Grade == selectedGrade).ToList();
            }

            return list;
        }

        private static int SafeParseInt(string s)
        {
            int v; return int.TryParse(s, out v) ? v : 0;
        }
    }
}
