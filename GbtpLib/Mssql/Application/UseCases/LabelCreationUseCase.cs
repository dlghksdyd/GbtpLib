using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // Combine checks similar to Reference LabelCreation flow
    public class LabelCreationUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrTypeRepository _btrTypeRepo;
        private readonly IMstBtrRepository _btrRepo;

        public LabelCreationUseCase(IUnitOfWork uow, IMstBtrTypeRepository btrTypeRepo, IMstBtrRepository btrRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrTypeRepo = btrTypeRepo ?? throw new ArgumentNullException(nameof(btrTypeRepo));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
        }

        public async Task<bool> CreateAsync(string labelId, int batteryTypeNo, string packModuleCode, string siteCode, string collectDate, string collectReason, CancellationToken ct = default(CancellationToken))
        {
            // Fetch battery type meta to validate/extract fields
            var type = await _btrTypeRepo.GetByNoAsync(batteryTypeNo, ct).ConfigureAwait(false);
            if (type == null) return false;

            try
            {
                var entity = new MstBtrEntity
                {
                    LabelId = labelId,
                    BatteryTypeNo = batteryTypeNo,
                    PackModuleCode = packModuleCode,
                    SiteCode = siteCode,
                    CollectDate = collectDate,
                    CollectReason = collectReason,
                    PrintYn = "N",
                    BatteryStatus = "01",
                    StoreInspFlag = "N",
                    EnergyInspFlag = "N",
                    DigInspFlag = "N",
                    UseYn = "Y",
                    RegDateTime = DateTime.UtcNow,
                };

                var affected = await _btrRepo.InsertAsync(entity, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
