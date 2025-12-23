using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class ItfCmdDataQueries : IItfCmdDataQueries
    {
        private readonly IAppDbContext _db;
        public ItfCmdDataQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<ItfCmdDataEntity>> GetPendingAsync(string cmdCode, string data1, CancellationToken ct = default(CancellationToken))
        {
            ct.ThrowIfCancellationRequested();

            var list = await _db.Set<ItfCmdDataEntity>()
                .Where(x => x.CommandCode == cmdCode && x.Data1 == data1 && x.IfFlag == "C")
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return list;
        }
    }
}
