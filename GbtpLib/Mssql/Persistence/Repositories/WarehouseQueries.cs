using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain.ReadModels;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class WarehouseQueries : IWarehouseQueries
    {
        private readonly IAppDbContext _db;
        public WarehouseQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<WarehouseCodeNameDto>> GetWarehousesAsync(string siteCode, string factCode, string whType, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var list = await _db.Set<MstWarehouseEntity>()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factCode && x.WarehouseType == whType && x.UseYn == "Y")
                .OrderBy(x => x.WarehouseCode)
                .Select(x => new WarehouseCodeNameDto { Code = x.WarehouseCode, Name = x.WarehouseName })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return list;
        }
    }
}
