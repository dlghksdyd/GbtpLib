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
    // Query-side repository for warehouse slots (CQRS read path)
    public class WarehouseQueries : ISlotQueryRepository
    {
        private readonly IAppDbContext _db;
        public WarehouseQueries(IAppDbContext db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public Task<IReadOnlyList<SlotInfoDto>> GetOutcomeWaitSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            return GetSlotsAsync(siteCode, factCode, whCode, ct);
        }

        public Task<IReadOnlyList<SlotInfoDto>> GetLoadingSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            return GetSlotsAsync(siteCode, factCode, whCode, ct);
        }

        public async Task<IReadOnlyList<GradeClassBatteryDbDto>> SearchGradeWarehouseBatteriesAsync(
            string siteCode,
            string factCode,
            string gradeWarehouseCode,
            string labelSubstring,
            string selectedGrade,
            DateTime startCollectionDate,
            DateTime endCollectionDate,
            string carManufacture,
            string carModel,
            string batteryManufacture,
            string releaseYear,
            string batteryType,
            CancellationToken ct = default(CancellationToken))
        {
            var slots = await GetSlotsAsync(siteCode, factCode, gradeWarehouseCode, ct).ConfigureAwait(false);

            var filtered = slots.Where(s =>
                (!string.IsNullOrEmpty(s.LabelId)) &&
                (string.IsNullOrEmpty(labelSubstring) || s.LabelId.Contains(labelSubstring)) &&
                (string.IsNullOrEmpty(selectedGrade) || string.Equals(s.InspectGrade, selectedGrade, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(s.CollectDate) || (startCollectionDate == DateTime.MinValue || CompareDateString(s.CollectDate, startCollectionDate) >= 0)) &&
                (string.IsNullOrEmpty(s.CollectDate) || (endCollectionDate == DateTime.MinValue || CompareDateString(s.CollectDate, endCollectionDate) <= 0)) &&
                (string.IsNullOrEmpty(carManufacture) || string.Equals(s.CarMakeName, carManufacture, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(carModel) || string.Equals(s.CarName, carModel, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(batteryManufacture) || string.Equals(s.BatteryMakeName, batteryManufacture, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(releaseYear) || string.Equals(s.CarReleaseYear, releaseYear, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(batteryType) || string.Equals(s.BatteryTypeName, batteryType, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            var result = filtered.Select(s => new GradeClassBatteryDbDto
            {
                Row = s.Row,
                Col = s.Col,
                Lvl = s.Lvl,
                LblId = s.LabelId,
                InspGrd = s.InspectGrade,
                SiteNm = s.SiteName,
                ColtDat = s.CollectDate,
                ColtResn = s.CollectReason,
                PackMdleCd = s.PackModuleCode,
                BtrTypeNm = s.BatteryTypeName,
                CarRelsYear = s.CarReleaseYear,
                CarMakeNm = s.CarMakeName,
                CarNm = s.CarName,
                BtrMakeNm = s.BatteryMakeName,
            }).ToList();

            return result;
        }

        private async Task<IReadOnlyList<SlotInfoDto>> GetSlotsAsync(string siteCode, string factCode, string whCode, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var inspSrc = _db.Set<QltBtrInspEntity>().AsNoTracking().Where(q => q.BatteryDiagStatus == "Y");

            var baseRows = await (
                from inv in _db.Set<InvWarehouseEntity>().AsNoTracking()
                join btr in _db.Set<MstBtrEntity>().AsNoTracking() on inv.LabelId equals btr.LabelId into btrJoin
                from btr in btrJoin.DefaultIfEmpty()
                join insp1 in inspSrc on inv.LabelId equals insp1.LabelId into inspJoin
                from insp in inspJoin
                    .OrderByDescending(i => i.InspectSeq)
                    .Take(1)
                    .DefaultIfEmpty()
                where inv.SiteCode == siteCode
                   && inv.FactoryCode == factCode
                   && inv.WarehouseCode == whCode
                   && (btr == null || btr.UseYn == "Y")
                orderby inv.Row, inv.Col, inv.Level
                select new
                {
                    inv.Row,
                    inv.Col,
                    Lvl = inv.Level,
                    inv.LabelId,
                    inv.StoreDiv,
                    InspectGrade = insp != null ? insp.InspectGrade : null,
                    SiteCode = inv.SiteCode,
                    CollectDate = btr != null ? btr.CollectDate : null,
                    CollectReason = btr != null ? btr.CollectReason : null,
                    PackModuleCode = btr != null ? btr.PackModuleCode : null,
                    BatteryTypeNo = btr != null ? (int?)btr.BatteryTypeNo : null,
                    PrintYn = btr != null ? btr.PrintYn : null,
                }
            ).ToListAsync(ct).ConfigureAwait(false);

            var siteCodes = baseRows.Select(r => r.SiteCode).Where(s => s != null).Distinct().ToList();
            var typeNos = baseRows.Select(r => r.BatteryTypeNo).Where(n => n.HasValue).Select(n => n.Value).Distinct().ToList();

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
                    t.BatteryMakeCode,
                    t.VoltMaxValue,
                    t.VoltMinValue,
                    t.AcirMaxValue,
                    t.InsulMinValue,
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

            var list = baseRows.Select(r =>
            {
                string siteName = null; siteDict.TryGetValue(r.SiteCode, out siteName);

                string batteryTypeName = null;
                string carReleaseYear = null;
                string carMakeName = null;
                string carName = null;
                string batteryMakeName = null;
                double voltMax = 0, voltMin = 0, acirMax = 0, insMin = 0;

                if (r.BatteryTypeNo.HasValue && btrTypeDict.TryGetValue(r.BatteryTypeNo.Value, out var t))
                {
                    batteryTypeName = t.BatteryTypeName;
                    carReleaseYear = t.CarReleaseYear;
                    if (t.CarMakeCode != null) carMakeDict.TryGetValue(t.CarMakeCode, out carMakeName);
                    if (t.CarCode != null) carDict.TryGetValue(t.CarCode, out carName);
                    if (t.BatteryMakeCode != null) btrMakeDict.TryGetValue(t.BatteryMakeCode, out batteryMakeName);
                    voltMax = (double?)(t.VoltMaxValue) ?? 0.0;
                    voltMin = (double?)(t.VoltMinValue) ?? 0.0;
                    acirMax = (double?)(t.AcirMaxValue) ?? 0.0;
                    insMin = (double?)(t.InsulMinValue) ?? 0.0;
                }

                return new SlotInfoDto
                {
                    Row = SafeParseInt(r.Row),
                    Col = SafeParseInt(r.Col),
                    Lvl = SafeParseInt(r.Lvl),
                    LabelId = r.LabelId,
                    StoreDiv = r.StoreDiv,
                    InspectGrade = r.InspectGrade,
                    SiteName = siteName,
                    CollectDate = r.CollectDate,
                    CollectReason = r.CollectReason,
                    PackModuleCode = r.PackModuleCode,
                    BatteryTypeName = batteryTypeName,
                    CarReleaseYear = carReleaseYear,
                    CarMakeName = carMakeName,
                    CarName = carName,
                    BatteryMakeName = batteryMakeName,
                    PrintYn = r.PrintYn,
                    VoltMaxValue = voltMax,
                    VoltMinValue = voltMin,
                    AcirMaxValue = acirMax,
                    InsulationMinValue = insMin,
                };
            }).ToList();

            return list;
        }

        private static int CompareDateString(string yyyymmdd, DateTime dt)
        {
            if (string.IsNullOrEmpty(yyyymmdd)) return 0;
            if (yyyymmdd.Length != 8) return 0;
            int strVal; if (!int.TryParse(yyyymmdd, out strVal)) return 0;
            int dtVal = dt.Year * 10000 + dt.Month * 100 + dt.Day;
            if (strVal == dtVal) return 0;
            return strVal < dtVal ? -1 : 1;
        }

        private static int SafeParseInt(string s)
        {
            int v; return int.TryParse(s, out v) ? v : 0;
        }
    }
}
