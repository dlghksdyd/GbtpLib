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
    public class QltBtrInspQueries : IQltBtrInspQueries
    {
        private readonly IAppDbContext _db;
        public QltBtrInspQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<string> GetLatestGradeAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var q = _db.Set<QltBtrInspEntity>().AsNoTracking()
                .Where(x => x.LabelId == labelId && x.BatteryDiagStatus == "Y")
                .OrderByDescending(x => x.InspectSeq)
                .Select(x => x.InspectGrade);

            var grade = await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
            return grade;
        }

        public async Task<IReadOnlyList<InoutInspectionInfoDto>> GetInoutInspectionInfosAsync(
            string siteCode,
            string factoryCode,
            string processCode,
            string machineCode,
            string inspKindGroupCode,
            string inspKindCode,
            string labelId,
            CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var inspBase = _db.Set<QltBtrInoutInspEntity>().AsNoTracking()
                .Where(q => q.SiteCode == siteCode
                         && q.FactoryCode == factoryCode
                         && q.ProcessCode == processCode
                         && q.MachineCode == machineCode
                         && q.InspKindGroupCode == inspKindGroupCode
                         && q.InspKindCode == inspKindCode
                         && q.LabelId == labelId)
                .OrderByDescending(q => q.InspectSeq);

            var query = from insp in inspBase
                        join btr in _db.Set<MstBtrEntity>().AsNoTracking() on insp.LabelId equals btr.LabelId into btrJoin
                        from btr in btrJoin.DefaultIfEmpty()
                        join btrType in _db.Set<MstBtrTypeEntity>().AsNoTracking() on btr.BatteryTypeNo equals btrType.BatteryTypeNo into btJoin
                        from btrType in btJoin.DefaultIfEmpty()
                        join carMake in _db.Set<MstCarMakeEntity>().AsNoTracking() on btrType.CarMakeCode equals carMake.CarMakeCode into cmJoin
                        from carMake in cmJoin.DefaultIfEmpty()
                        join car in _db.Set<MstCarEntity>().AsNoTracking() on btrType.CarCode equals car.CarCode into cJoin
                        from car in cJoin.DefaultIfEmpty()
                        join btrMake in _db.Set<MstBtrMakeEntity>().AsNoTracking() on btrType.BatteryMakeCode equals btrMake.BatteryMakeCode into bmJoin
                        from btrMake in bmJoin.DefaultIfEmpty()
                        where btr == null || btr.UseYn == "Y"
                        select new { insp, btr, btrType, carMake, car, btrMake };

            var list = await query
                .Select(x => new
                {
                    x.insp.LabelId,
                    x.insp.InspectSeq,
                    InspectValueJson = x.insp.InspectValue,
                    InspectResultEnum = x.insp.InspectResult,
                    InspectStart = x.insp.InspectStart,
                    InspectEnd = x.insp.InspectEnd,
                    x.insp.Note,
                    x.insp.RegId,
                    CollectDate = x.btr != null ? x.btr.CollectDate : null,
                    BatteryTypeName = x.btrType != null ? x.btrType.BatteryTypeName : null,
                    CarReleaseYear = x.btrType != null ? x.btrType.CarReleaseYear : null,
                    CarMakeName = x.carMake != null ? x.carMake.CarMakeName : null,
                    CarName = x.car != null ? x.car.CarName : null,
                    BatteryMakeName = x.btrMake != null ? x.btrMake.BatteryMakeName : null,
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var result = new List<InoutInspectionInfoDto>(list.Count);
            foreach (var i in list)
            {
                int seqInt = 0; int.TryParse(i.InspectSeq, out seqInt);
                int inspResultEnum = 0; int.TryParse(i.InspectResultEnum, out inspResultEnum);
                result.Add(new InoutInspectionInfoDto
                {
                    LabelId = i.LabelId,
                    InspectSeqInt = seqInt,
                    InspectValueJson = i.InspectValueJson,
                    InspectResultEnum = inspResultEnum,
                    InspectStart = i.InspectStart?.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    InspectEnd = i.InspectEnd?.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    Note = i.Note,
                    RegId = i.RegId,
                    CollectDate = i.CollectDate,
                    BatteryTypeName = i.BatteryTypeName,
                    CarReleaseYear = i.CarReleaseYear,
                    CarMakeName = i.CarMakeName,
                    CarName = i.CarName,
                    BatteryMakeName = i.BatteryMakeName,
                });
            }

            return result;
        }
    }
}
