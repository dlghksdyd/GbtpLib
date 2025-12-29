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
    // Single queries class for warehouse metadata and layout
    public class WarehouseQueries : IWarehouseQueries, IWarehouseLayoutQueries
    {
        private readonly IAppDbContext _db;
        public WarehouseQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<WarehouseCodeNameDto>> GetWarehousesAsync(string siteCode, string factCode, string whType, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var list = await _db.Set<MstWarehouseEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factCode && x.WarehouseType == whType && x.UseYn == "Y")
                .OrderBy(x => x.WarehouseCode)
                .Select(x => new WarehouseCodeNameDto { Code = x.WarehouseCode, Name = x.WarehouseName })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return list;
        }

        public async Task<IReadOnlyList<WarehouseSlotLayoutDto>> GetLayoutAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var list = await _db.Set<InvWarehouseEntity>().AsNoTracking()
                .Where(x => x.SiteCode == siteCode && x.FactoryCode == factCode && x.WarehouseCode == whCode)
                .OrderBy(x => x.Level).ThenBy(x => x.Row).ThenBy(x => x.Col)
                .Select(x => new WarehouseSlotLayoutDto
                {
                    Row = SafeParseInt(x.Row),
                    Col = SafeParseInt(x.Col),
                    Lvl = SafeParseInt(x.Level),
                    LabelId = x.LabelId,
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return list;
        }

        private static int SafeParseInt(string s) { int v; return int.TryParse(s, out v) ? v : 0; }
    }
}
