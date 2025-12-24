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
            var list = await _db.Set<MstSiteEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.SiteCode, Name = x.SiteName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<CodeNameDto>> GetFactoriesAsync(string siteCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<MstFactoryEntity>()
                .Where(x => x.SiteCode == siteCode && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => new CodeNameDto { Code = x.FactoryCode, Name = x.FactoryName })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstCarMakeEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarMakeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstCarEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrMakeEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.BatteryMakeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrTypeEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.BatteryTypeName)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default(CancellationToken))
        {
            return await _db.Set<MstBtrTypeEntity>()
                .Where(x => x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CarReleaseYear)
                .Distinct()
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }
    }
}
