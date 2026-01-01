using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates label-related operations (create/delete, metadata, validation, slot assignment, flows).
    /// </summary>
    public class LabelManagementUseCases
    {
        private readonly IMstBtrRepository _btrRepo;
        private readonly IInvWarehouseRepository _whRepo;
        private readonly ILabelCreationQueries _labelQueries;
        private readonly IMstBtrTypeRepository _btrTypeRepo;

        public LabelManagementUseCases(
            IMstBtrRepository btrRepo,
            IInvWarehouseRepository whRepo,
            ILabelCreationQueries labelQueries,
            IMstBtrTypeRepository btrTypeRepo)
        {
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
            _labelQueries = labelQueries ?? throw new ArgumentNullException(nameof(labelQueries));
            _btrTypeRepo = btrTypeRepo ?? throw new ArgumentNullException(nameof(btrTypeRepo));
        }

        // Basic create/delete
        public async Task<bool> CreateLabelAsync(MstBtrEntity entity, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace("CreateLabel start");
                var affected = await _btrRepo.InsertAsync(entity, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"CreateLabel done rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"CreateLabel error elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteLabelAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"DeleteLabel start label={labelId}");
                var affected = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"DeleteLabel done label={labelId}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"DeleteLabel error label={labelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        public Task<int> GetNextVersionAsync(string collectDate, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                return _btrRepo.GetNextVersionAsync(collectDate, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error("LabelManagementUseCases.GetNextVersionAsync failed.", ex);
                throw;
            }
        }

        // Create and assign slot
        public async Task<bool> CreateLabelAndAssignSlotAsync(MstBtrEntity label, WarehouseSlotUpdateDto slot, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"CreateLabelAndAssignSlot start label={label?.LabelId}");
                var ins = await _btrRepo.InsertAsync(label, ct).ConfigureAwait(false);
                if (ins < 1) { sw.Stop(); AppLog.Info($"CreateLabelAndAssignSlot done label={label?.LabelId}, rows=0, elapsedMs={sw.ElapsedMilliseconds}"); return false; }

                var upd = await _whRepo.UpdateLabelAndGradeAsync(slot, ct).ConfigureAwait(false);
                if (upd < 1)
                {
                    try { await _btrRepo.DeleteAsync(label.LabelId, ct).ConfigureAwait(false); } catch { }
                    sw.Stop();
                    AppLog.Info($"CreateLabelAndAssignSlot rollback label={label?.LabelId}, elapsedMs={sw.ElapsedMilliseconds}");
                    return false;
                }
                sw.Stop();
                AppLog.Info($"CreateLabelAndAssignSlot done label={label?.LabelId}, rows={ins + upd}, elapsedMs={sw.ElapsedMilliseconds}");
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"CreateLabelAndAssignSlot error label={label?.LabelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        // Delete flow (delete + clear slots)
        public async Task<bool> DeleteLabelFlowAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"DeleteLabelFlow start label={labelId}");
                var del = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                if (del < 1) { sw.Stop(); AppLog.Info($"DeleteLabelFlow done label={labelId}, rows=0, elapsedMs={sw.ElapsedMilliseconds}"); return false; }

                await _whRepo.ClearLabelByLabelIdAsync(labelId, siteCode, factoryCode, warehouseCode, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"DeleteLabelFlow done label={labelId}, rows={del}, elapsedMs={sw.ElapsedMilliseconds}");
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"DeleteLabelFlow error label={labelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        // Metadata for label creation UI
        public async Task<IReadOnlyList<LabelCreationInfoDto>> GetLabelCreationMetadataAsync(CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace("GetLabelCreationMetadata start");
                var list = await _labelQueries.GetLabelCreationInfosAsync(ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetLabelCreationMetadata done count={(list?.Count ?? 0)}, elapsedMs={sw.ElapsedMilliseconds}");
                return list;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetLabelCreationMetadata error elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        // Validation + create (mirrors LabelCreationUseCase)
        public async Task<string> GetPackModuleCodeAsync(int batteryTypeNo, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"GetPackModuleCode start typeNo={batteryTypeNo}");
                var type = await _btrTypeRepo.GetByNoAsync(batteryTypeNo, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"GetPackModuleCode done typeNo={batteryTypeNo}, elapsedMs={sw.ElapsedMilliseconds}");
                return type?.PackModuleCode;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"GetPackModuleCode error typeNo={batteryTypeNo}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        public async Task<bool> CreateWithValidationAsync(string labelId, int batteryTypeNo, string packModuleCode, string siteCode, string collectDate, string collectReason, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"CreateWithValidation start label={labelId}, typeNo={batteryTypeNo}");
                var type = await _btrTypeRepo.GetByNoAsync(batteryTypeNo, ct).ConfigureAwait(false);
                if (type == null) { sw.Stop(); AppLog.Info($"CreateWithValidation done label={labelId}, rows=0, elapsedMs={sw.ElapsedMilliseconds}"); return false; }

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
                sw.Stop();
                AppLog.Info($"CreateWithValidation done label={labelId}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"CreateWithValidation error label={labelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }

        // Set print flag
        public async Task<bool> SetPrintedAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                AppLog.Trace($"SetPrinted start label={labelId}");
                var affected = await _btrRepo.UpdatePrintYnAsync(labelId, "Y", ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"SetPrinted done label={labelId}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return affected > 0;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"SetPrinted error label={labelId}, elapsedMs={sw.ElapsedMilliseconds}", ex);
                throw;
            }
        }
    }
}
