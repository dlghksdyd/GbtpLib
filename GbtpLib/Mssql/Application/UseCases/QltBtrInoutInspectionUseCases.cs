using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class QltBtrInoutInspectionUseCases
    {
        private readonly IAppDbContext _db;
        public QltBtrInoutInspectionUseCases(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<QltBtrInoutInspEntity>> GetInspectionsAsync(
            string site, string fact, string prcs, string mc,
            string kindGroup, string kind, string labelId,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _db.Set<QltBtrInoutInspEntity>().AsNoTracking()
                    .Where(x => x.SiteCode == site
                             && x.FactoryCode == fact
                             && x.ProcessCode == prcs
                             && x.MachineCode == mc
                             && x.InspKindGroupCode == kindGroup
                             && x.InspKindCode == kind
                             && (labelId == string.Empty || x.LabelId == labelId))
                    .OrderByDescending(x => x.InspectSeq)
                    .ToListAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("QltBtrInoutInspectionUseCases.GetInspectionsAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> InsertAsync(QltBtrInoutInspEntity entity, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                _db.Set<QltBtrInoutInspEntity>().Add(entity);
                var affected = await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("QltBtrInoutInspectionUseCases.InsertAsync failed.", ex);
                throw;
            }
        }
    }
}
