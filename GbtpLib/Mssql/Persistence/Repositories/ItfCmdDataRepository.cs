using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class ItfCmdDataRepository : IItfCmdDataRepository
    {
        private readonly IAppDbContext _db;
        public ItfCmdDataRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> EnqueueAsync(EIfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = new ItfCmdDataEntity
            {
                IfUid = DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                IfDate = DateTime.UtcNow.ToString("yyyyMMdd"),
                CommandCode = cmd.ToString(),
                Data1 = data1,
                Data2 = data2,
                Data3 = data3,
                Data4 = data4,
                IfFlag = "C",
                RequestTime = DateTime.UtcNow,
                RequestSystem = requestSystem,
            };

            _db.Set<ItfCmdDataEntity>().Add(entity);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var list = await _db.Set<ItfCmdDataEntity>()
                .Where(x => x.CommandCode == cmd.ToString() && x.Data1 == data1 && x.IfFlag == "C")
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var row in list)
            {
                row.IfFlag = "Y";
                row.ResponseTime = DateTime.UtcNow;
            }

            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
