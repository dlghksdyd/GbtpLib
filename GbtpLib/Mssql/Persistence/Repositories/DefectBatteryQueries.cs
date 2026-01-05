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
using GbtpLib.Logging;

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
            try
            {
                ct.ThrowIfCancellationRequested();

                // 1) Base rows (INV + MST_BTR + latest DIAG insp only), minimal joins
                var latestDiagInsp = _db.Set<QltBtrInspEntity>().AsNoTracking()
                    .Where(q => q.BatteryDiagStatus == "Y" && q.InspKindCode == "DIAG");

                var baseRows = await (
                    from inv in _db.Set<InvWarehouseEntity>().AsNoTracking()
                    join btr in _db.Set<MstBtrEntity>().AsNoTracking() on inv.LabelId equals btr.LabelId into btrJoin
                    from btr in btrJoin.DefaultIfEmpty()
                    // latest insp per label via filtered subquery
                    from insp in latestDiagInsp.Where(i => i.LabelId == inv.LabelId)
                                               .OrderByDescending(i => i.InspectSeq)
                                               .Take(1)
                                               .DefaultIfEmpty()
                    where inv.SiteCode == site && inv.FactoryCode == fact && inv.WarehouseCode == defectWh
                       && (btr == null || btr.UseYn == "Y")
                    select new
                    {
                        inv.Row,
                        inv.Col,
                        inv.Level,
                        inv.LabelId,
                        SiteCode = inv.SiteCode,
                        CollectDate = btr != null ? btr.CollectDate : null,
                        CollectReason = btr != null ? btr.CollectReason : null,
                        DigInspFlag = btr != null ? btr.DigInspFlag : null,
                        PackModuleCode = btr != null ? btr.PackModuleCode : null,
                        BatteryTypeNo = btr != null ? (int?)btr.BatteryTypeNo : null,
                        Grade = insp != null ? insp.InspectGrade : null,
                    }
                ).ToListAsync(ct).ConfigureAwait(false);

                // Exclude rows with null or empty LabelId
                baseRows = baseRows.Where(x => !string.IsNullOrEmpty(x.LabelId)).ToList();

                // Early filtering on label like and collection date
                if (!string.IsNullOrEmpty(labelLike)) baseRows = baseRows.Where(x => x.LabelId != null && x.LabelId.Contains(labelLike)).ToList();
                if (!string.IsNullOrEmpty(startCollectionDate)) baseRows = baseRows.Where(x => string.IsNullOrEmpty(x.CollectDate) || x.CollectDate.CompareTo(startCollectionDate) >= 0).ToList();
                if (!string.IsNullOrEmpty(endCollectionDate)) baseRows = baseRows.Where(x => string.IsNullOrEmpty(x.CollectDate) || x.CollectDate.CompareTo(endCollectionDate) <= 0).ToList();

                // 2) Collect keys for lookups
                var siteCodes = baseRows.Select(r => r.SiteCode).Where(s => s != null).Distinct().ToList();
                var typeNos = baseRows.Select(r => r.BatteryTypeNo).Where(n => n.HasValue).Select(n => n.Value).Distinct().ToList();

                // 3) Lookup tables fetched separately
                var siteDict = await _db.Set<MstSiteEntity>().AsNoTracking()
                    .Where(s => siteCodes.Contains(s.SiteCode))
                    .Select(s => new { s.SiteCode, s.SiteName })
                    .ToDictionaryAsync(x => x.SiteCode, x => x.SiteName, ct).ConfigureAwait(false);

                var btrTypes = await _db.Set<MstBtrTypeEntity>().AsNoTracking()
                    .Where(t => typeNos.Contains(t.BatteryTypeNo))
                    .Select(t => new
                    {
                        t.BatteryTypeNo,
                        t.BatteryTypeName,
                        t.CarReleaseYear,
                        t.CarMakeCode,
                        t.CarCode,
                        t.BatteryMakeCode
                    })
                    .ToListAsync(ct).ConfigureAwait(false);

                var carMakeCodes = btrTypes.Select(t => t.CarMakeCode).Where(c => c != null).Distinct().ToList();
                var carCodes = btrTypes.Select(t => t.CarCode).Where(c => c != null).Distinct().ToList();
                var btrMakeCodes = btrTypes.Select(t => t.BatteryMakeCode).Where(c => c != null).Distinct().ToList();

                var btrTypeDict = btrTypes.ToDictionary(t => t.BatteryTypeNo, t => t);

                var carMakeDict = await _db.Set<MstCarMakeEntity>().AsNoTracking()
                    .Where(c => carMakeCodes.Contains(c.CarMakeCode))
                    .Select(c => new { c.CarMakeCode, c.CarMakeName })
                    .ToDictionaryAsync(x => x.CarMakeCode, x => x.CarMakeName, ct).ConfigureAwait(false);

                var carDict = await _db.Set<MstCarEntity>().AsNoTracking()
                    .Where(c => carCodes.Contains(c.CarCode))
                    .Select(c => new { c.CarCode, c.CarName })
                    .ToDictionaryAsync(x => x.CarCode, x => x.CarName, ct).ConfigureAwait(false);

                var btrMakeDict = await _db.Set<MstBtrMakeEntity>().AsNoTracking()
                    .Where(m => btrMakeCodes.Contains(m.BatteryMakeCode))
                    .Select(m => new { m.BatteryMakeCode, m.BatteryMakeName })
                    .ToDictionaryAsync(x => x.BatteryMakeCode, x => x.BatteryMakeName, ct).ConfigureAwait(false);

                // 4) Final DTO composition in memory
                var list = baseRows.Select(r =>
                {
                    string siteNameVal = null; siteDict.TryGetValue(r.SiteCode, out siteNameVal);

                    string batteryTypeNameVal = null;
                    string carReleaseYearVal = null;
                    string carMakeNameVal = null;
                    string carNameVal = null;
                    string batteryMakeNameVal = null;

                    if (r.BatteryTypeNo.HasValue && btrTypeDict.TryGetValue(r.BatteryTypeNo.Value, out var t))
                    {
                        batteryTypeNameVal = t.BatteryTypeName;
                        carReleaseYearVal = t.CarReleaseYear;
                        if (t.CarMakeCode != null) carMakeDict.TryGetValue(t.CarMakeCode, out carMakeNameVal);
                        if (t.CarCode != null) carDict.TryGetValue(t.CarCode, out carNameVal);
                        if (t.BatteryMakeCode != null) btrMakeDict.TryGetValue(t.BatteryMakeCode, out batteryMakeNameVal);
                    }

                    return new DefectBatteryDto
                    {
                        Row = SafeParseInt(r.Row),
                        Col = SafeParseInt(r.Col),
                        Lvl = SafeParseInt(r.Level),
                        LabelId = r.LabelId,
                        SiteName = siteNameVal,
                        CollectDate = r.CollectDate,
                        CollectReason = r.CollectReason,
                        DigInspFlag = r.DigInspFlag,
                        PackModuleCode = r.PackModuleCode,
                        BatteryTypeName = batteryTypeNameVal,
                        CarReleaseYear = carReleaseYearVal,
                        CarMakeName = carMakeNameVal,
                        CarName = carNameVal,
                        BatteryMakeName = batteryMakeNameVal,
                        Grade = r.Grade,
                    };
                }).ToList();

                // 5) Apply remaining filters in memory
                if (!string.IsNullOrEmpty(carMakeName)) list = list.Where(x => x.CarMakeName == carMakeName).ToList();
                if (!string.IsNullOrEmpty(carName)) list = list.Where(x => x.CarName == carName).ToList();
                if (!string.IsNullOrEmpty(batteryMakeName)) list = list.Where(x => x.BatteryMakeName == batteryMakeName).ToList();
                if (!string.IsNullOrEmpty(releaseYear)) list = list.Where(x => x.CarReleaseYear == releaseYear).ToList();
                if (!string.IsNullOrEmpty(batteryTypeName)) list = list.Where(x => x.BatteryTypeName == batteryTypeName).ToList();
                if (!string.IsNullOrEmpty(selectedGrade)) list = list.Where(x => x.Grade == selectedGrade).ToList();

                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("DefectBatteryQueries.SearchAsync failed.", ex);
                return new List<DefectBatteryDto>();
            }
        }

        private static int SafeParseInt(string s)
        {
            int v; return int.TryParse(s, out v) ? v : 0;
        }
    }
}
