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
    public class MetadataQueries : IMetadataQueries
    {
        private readonly IAppDbContext _db;
        public MetadataQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetSitesAsync(CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstSiteEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.SiteCode, Name = x.SiteName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetFactoriesAsync(string siteCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstFactoryEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.FactoryCode, Name = x.FactoryName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetWarehousesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstWarehouseEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factoryCode && x.UseYn == "Y")
                .OrderBy(x => x.WarehouseCode)
                .Select(x => new CodeNameDto { Code = x.WarehouseCode, Name = x.WarehouseName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetProcessesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstProcessEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factoryCode && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.ProcessCode, Name = x.ProcessName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetMachinesAsync(string siteCode, string factoryCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstMachineEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factoryCode && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.MachineCode, Name = x.MachineName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindGroupsAsync(CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstInspKindGroupEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.InspKindGroupCode, Name = x.InspKindGroupName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetInspKindsAsync(string inspKindGroupCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstInspKindEntity>().AsNoTracking()
                .Where(x => x.InspKindGroupCode == inspKindGroupCode && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.InspKindCode, Name = x.InspKindName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstCarMakeEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarMakeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstCarEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrMakeEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.BatteryMakeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrTypeEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.BatteryTypeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrTypeEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarReleaseYear)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetGradeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstCodeEntity>().AsNoTracking()
                .Where(x => x.CodeGroup == "INV002" && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CodeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<BatteryTypeYearDto>> GetBatteryTypesWithYearAsync(CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstBtrTypeEntity>().AsNoTracking()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new BatteryTypeYearDto { BatteryTypeName = x.BatteryTypeName, CarReleaseYear = x.CarReleaseYear })
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }
    }
}
