using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates label-related operations (create/delete, metadata, validation, slot assignment, flows, lookup).
    /// </summary>
    public class LabelUseCases
    {
        private readonly ILabelCommands _btrRepo;            // command
        private readonly IWarehouseCommands _whRepo;         // command
        private readonly ILabelQueries _labelQueries;        // read
        private readonly IMstBtrTypeQueries _btrTypeQueries; // read

        public LabelUseCases(
            ILabelCommands btrRepo,
            IWarehouseCommands whRepo,
            ILabelQueries labelQueries,
            IMstBtrTypeQueries btrTypeQueries)
        {
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
            _labelQueries = labelQueries ?? throw new ArgumentNullException(nameof(labelQueries));
            _btrTypeQueries = btrTypeQueries ?? throw new ArgumentNullException(nameof(btrTypeQueries));
        }

        public Task<int> GetNextVersionAsync(string collectDate, CancellationToken ct = default(CancellationToken))
        {
            try { return _labelQueries.GetNextVersionAsync(collectDate, ct); }
            catch (Exception ex) { AppLog.Error($"LabelUseCases.GetNextVersionAsync failed. collectDate={collectDate}", ex); throw; }
        }

        public async Task<bool> UpdatePrintYnAsync(string labelId, string printYn, CancellationToken ct = default(CancellationToken))
        {
            try { var affected = await _btrRepo.UpdatePrintYnAsync(labelId, printYn, ct).ConfigureAwait(false); return affected > 0; }
            catch (Exception ex) { AppLog.Error($"LabelUseCases.UpdatePrintYnAsync failed. labelId={labelId}, printYn={printYn}", ex); throw; }
        }

        public async Task<bool> CreateLabelAndAssignSlotAsync(MstBtrEntity label, WarehouseSlotUpdateDto slot, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ins = await _btrRepo.InsertAsync(label, ct).ConfigureAwait(false);
                if (ins < 1) return false;
                var upd = await _whRepo.UpdateLabelAndGradeAsync(slot, ct).ConfigureAwait(false);
                if (upd < 1)
                {
                    try { await _btrRepo.DeleteAsync(label.LabelId, ct).ConfigureAwait(false); } catch { }
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error($"LabelUseCases.CreateLabelAndAssignSlotAsync failed. labelId={label?.LabelId}, site={slot?.SiteCode}, factory={slot?.FactoryCode}, warehouse={slot?.WarehouseCode}, row={slot?.Row}, col={slot?.Col}, lvl={slot?.Level}", ex);
                throw;
            }
        }

        public async Task<bool> DeleteLabelFlowAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var del = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                if (del < 1) return false;
                await _whRepo.ClearLabelByLabelIdAsync(labelId, siteCode, factoryCode, warehouseCode, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error($"LabelUseCases.DeleteLabelFlowAsync failed. labelId={labelId}, site={siteCode}, factory={factoryCode}, warehouse={warehouseCode}", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<LabelCreationInfoDto>> GetLabelCreationMetadataAsync(CancellationToken ct = default(CancellationToken))
        {
            try { return await _labelQueries.GetLabelCreationInfosAsync(ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error("LabelUseCases.GetLabelCreationMetadataAsync failed.", ex); throw; }
        }

        public async Task<string> GetPackModuleCodeAsync(int batteryTypeNo, CancellationToken ct = default(CancellationToken))
        {
            try { var type = await _btrTypeQueries.GetByNoAsync(batteryTypeNo, ct).ConfigureAwait(false); return type?.PackModuleCode; }
            catch (Exception ex) { AppLog.Error($"LabelUseCases.GetPackModuleCodeAsync failed. batteryTypeNo={batteryTypeNo}", ex); throw; }
        }

        public async Task<LabelInfoDto> GetLabelInfoByLabelIdAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            try { return await _labelQueries.GetByLabelIdAsync(labelId, ct).ConfigureAwait(false); }
            catch (Exception ex) { AppLog.Error($"LabelUseCases.GetLabelInfoByLabelIdAsync failed. labelId={labelId}", ex); throw; }
        }
    }
}
