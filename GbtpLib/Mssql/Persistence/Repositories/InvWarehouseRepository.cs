using System;
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
    public class InvWarehouseRepository : IInvWarehouseRepository
    {
        private readonly IAppDbContext _db;

        public InvWarehouseRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<int> UpdateLabelAndGradeAsync(WarehouseSlotUpdateDto dto, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = await _db.Set<InvWarehouseEntity>()
                .FirstOrDefaultAsync(x => x.SiteCode == dto.SiteCode
                                       && x.FactoryCode == dto.FactoryCode
                                       && x.WarehouseCode == dto.WarehouseCode
                                       && x.Row == dto.Row
                                       && x.Col == dto.Col
                                       && x.Level == dto.Level, cancellationToken)
                                       .ConfigureAwait(false);
            if (entity == null) return 0;

            entity.LabelId = dto.LabelId ?? string.Empty;
            entity.LoadGrade = dto.LoadGrade ?? string.Empty;

            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = await _db.Set<InvWarehouseEntity>()
                .FirstOrDefaultAsync(x => x.SiteCode == key.SiteCode
                                       && x.FactoryCode == key.FactoryCode
                                       && x.WarehouseCode == key.WarehouseCode
                                       && x.Row == key.Row
                                       && x.Col == key.Col
                                       && x.Level == key.Level, cancellationToken)
                                       .ConfigureAwait(false);
            if (entity == null) return 0;

            // 이미 비어있다면 반영된 것으로 간주
            if (string.IsNullOrEmpty(entity.LabelId) && string.IsNullOrEmpty(entity.LoadGrade))
            {
                return 1;
            }

            entity.LabelId = string.Empty;
            entity.LoadGrade = string.Empty;

            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ClearLabelByLabelIdAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // LabelId is unique; fetch single entity
            var query = _db.Set<InvWarehouseEntity>().AsNoTracking().Where(x => x.LabelId == labelId);
            if (!string.IsNullOrEmpty(siteCode)) query = query.Where(x => x.SiteCode == siteCode);
            if (!string.IsNullOrEmpty(factoryCode)) query = query.Where(x => x.FactoryCode == factoryCode);
            if (!string.IsNullOrEmpty(warehouseCode)) query = query.Where(x => x.WarehouseCode == warehouseCode);

            var entityKeyOnly = await query.Select(x => new { x.SiteCode, x.FactoryCode, x.WarehouseCode, x.Row, x.Col, x.Level })
                                           .FirstOrDefaultAsync(cancellationToken)
                                           .ConfigureAwait(false);
            if (entityKeyOnly == null) return 0;

            // Reattach tracked entity by key to update
            var entity = await _db.Set<InvWarehouseEntity>()
                .FirstOrDefaultAsync(x => x.SiteCode == entityKeyOnly.SiteCode
                                       && x.FactoryCode == entityKeyOnly.FactoryCode
                                       && x.WarehouseCode == entityKeyOnly.WarehouseCode
                                       && x.Row == entityKeyOnly.Row
                                       && x.Col == entityKeyOnly.Col
                                       && x.Level == entityKeyOnly.Level, cancellationToken)
                                       .ConfigureAwait(false);
            if (entity == null) return 0;

            // If already empty, consider as affected per business rule
            if (string.IsNullOrEmpty(entity.LabelId) && string.IsNullOrEmpty(entity.LoadGrade))
            {
                return 1;
            }

            entity.LabelId = string.Empty;
            entity.LoadGrade = string.Empty;

            return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
