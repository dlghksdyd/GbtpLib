using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Combines validations and inserts to create a label similarly to the reference flow.
    /// <para>
    /// Return semantics: <c>true</c> when insert affected &gt; 0 after successful battery type validation; otherwise <c>false</c>. Exceptions are propagated.
    /// </para>
    /// </summary>
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

        /// <summary>
        /// Creates the label after validating the battery type.
        /// </summary>
        public async Task<bool> CreateAsync(string labelId, int batteryTypeNo, string packModuleCode, string siteCode, string collectDate, string collectReason, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                // Fetch battery type meta to validate/extract fields
                var type = await _btrTypeRepo.GetByNoAsync(batteryTypeNo, ct).ConfigureAwait(false);
                if (type == null) return false;

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
            catch (Exception ex)
            {
                AppLog.Error("LabelCreationUseCase.CreateAsync failed.", ex);
                throw;
            }
        }
    }
}
