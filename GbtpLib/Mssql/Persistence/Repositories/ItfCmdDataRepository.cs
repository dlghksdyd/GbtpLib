using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class ItfCmdDataRepository : Abstractions.IItfCmdDataRepository
    {
        private readonly IAppDbContext _db;

        public ItfCmdDataRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> EnqueueAsync(IfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Generate IF_UID and IF_DATE similar to legacy convention
            var now = DateTime.UtcNow; // store in UTC or adjust per requirement
            var entity = new ItfCmdDataEntity
            {
                IfUid = Guid.NewGuid().ToString("N").Substring(0, 20),
                IfDate = now.ToString("yyyyMMdd"),
                CommandCode = cmd.ToString(),
                Data1 = data1,
                Data2 = data2,
                Data3 = data3,
                Data4 = data4,
                IfFlag = IfFlag.C.ToString(),
                RequestTime = now,
                RequestSystem = requestSystem,
            };

            _db.Set<ItfCmdDataEntity>().Add(entity);
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> AcknowledgeAsync(IfCmd cmd, string data1, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = await _db.Set<ItfCmdDataEntity>()
                .FirstOrDefaultAsync(x => x.CommandCode == cmd.ToString() && x.Data1 == data1 && x.IfFlag == IfFlag.C.ToString(), cancellationToken)
                .ConfigureAwait(false);
            if (entity == null) return 0;

            entity.IfFlag = IfFlag.Y.ToString();
            entity.ResponseTime = DateTime.UtcNow;
            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
