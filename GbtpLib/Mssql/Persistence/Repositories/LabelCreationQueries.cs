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
    public class LabelCreationQueries : ILabelCreationQueries
    {
        private readonly IAppDbContext _db;
        public LabelCreationQueries(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<LabelCreationInfoDto>> GetLabelCreationInfosAsync(CancellationToken ct = default(CancellationToken))
        {
            var query = from type in _db.Set<MstBtrTypeEntity>()
                        join carMake in _db.Set<MstCarMakeEntity>() on type.CarMakeCode equals carMake.CarMakeCode
                        join car in _db.Set<MstCarEntity>() on type.CarCode equals car.CarCode
                        join btrMake in _db.Set<MstBtrMakeEntity>() on type.BatteryMakeCode equals btrMake.BatteryMakeCode
                        where type.UseYn == "Y"
                        orderby type.ListOrder
                        select new LabelCreationInfoDto
                        {
                            CarMakeCode = carMake.CarMakeCode,
                            CarMakeName = carMake.CarMakeName,
                            CarCode = car.CarCode,
                            CarName = car.CarName,
                            BatteryMakeCode = btrMake.BatteryMakeCode,
                            BatteryMakeName = btrMake.BatteryMakeName,
                            CarReleaseYear = type.CarReleaseYear,
                            BatteryTypeNo = type.BatteryTypeNo,
                            BatteryTypeSelectCode = type.BatteryTypeSelectCode,
                            BatteryTypeName = type.BatteryTypeName,
                        };

            var list = await query.ToListAsync(ct).ConfigureAwait(false);
            return list;
        }
    }
}
