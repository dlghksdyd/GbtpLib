using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Logging;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public interface ILabelInfoLookupUseCase
    {
        Task<LabelInfoDto> GetByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken));
    }

    /// <summary>
    /// Provides label metadata lookup by LabelId (LBL_ID).
    /// Returns COLT_DAT, CAR_MAKE_NM, CAR_NM, CAR_RELS_YEAR, BTR_MAKE_NM, BTR_TYPE_NM.
    /// </summary>
    public class LabelInfoLookupUseCase : ILabelInfoLookupUseCase
    {
        private readonly IAppDbContext _db;
        public LabelInfoLookupUseCase(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Lookup label metadata for the given labelId.
        /// </summary>
        public async Task<LabelInfoDto> GetByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(labelId)) return new LabelInfoDto();

                var query = _db.Set<MstBtrEntity>().AsNoTracking()
                    .Where(b => b.LabelId == labelId)
                    .Join(_db.Set<MstBtrTypeEntity>().AsNoTracking(), b => b.BatteryTypeNo, t => t.BatteryTypeNo,
                        (b, t) => new
                        {
                            b.CollectDate,
                            TypeName = t.BatteryTypeName,
                            t.CarReleaseYear,
                            CarMakeName = t.CarMake != null ? t.CarMake.CarMakeName : null,
                            CarName = t.Car != null ? t.Car.CarName : null,
                            BatteryMakeName = t.BatteryMake != null ? t.BatteryMake.BatteryMakeName : null
                        });

                var item = await query.FirstOrDefaultAsync(ct).ConfigureAwait(false);
                if (item == null) return new LabelInfoDto();

                return new LabelInfoDto
                {
                    CollectionDate = item.CollectDate ?? string.Empty,
                    CarMakeNm = item.CarMakeName ?? string.Empty,
                    CarNm = item.CarName ?? string.Empty,
                    CarRelsYear = item.CarReleaseYear ?? string.Empty,
                    BtrMakeNm = item.BatteryMakeName ?? string.Empty,
                    BtrTypeNm = item.TypeName ?? string.Empty,
                };
            }
            catch (Exception ex)
            {
                AppLog.Error($"LabelInfoLookupUseCase.GetByLabelIdAsync failed. labelId={labelId}", ex);
                throw;
            }
        }
    }
}
