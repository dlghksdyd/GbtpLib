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
    public class WarehouseLayoutQueries : IWarehouseLayoutQueries
    {
        private readonly IAppDbContext _db;
        public WarehouseLayoutQueries(IAppDbContext db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public async Task<IReadOnlyList<WarehouseSlotLayoutDto>> GetLayoutAsync(string siteCode, string factCode, string whCode, CancellationToken ct = default(CancellationToken))
        {
            var list = await _db.Set<InvWarehouseEntity>()
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
