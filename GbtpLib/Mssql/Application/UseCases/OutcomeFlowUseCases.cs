using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using GbtpLib.Mssql.Domain; // Moved DTO here

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Outcome-related consolidated use cases.
    /// Provides search and flow operations backed by queries.
    /// </summary>
    public class OutcomeFlowUseCases
    {
        private readonly IInvWarehouseRepository _warehouseRepo;
        private readonly IStoredProcedureExecutor _sp;
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;
        // Add query repository for reading warehouse slot/battery info
        private readonly ISlotQueryRepository _slotQueries;

        public OutcomeFlowUseCases(IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor spExec, IItfCmdDataRepository cmdRepo, IItfCmdDataQueries cmdQueries)
        {
            _warehouseRepo = warehouseRepo;
            _sp = spExec;
            _repo = cmdRepo;
            _queries = cmdQueries;
        }

        // Overload allowing search capabilities via slot query repository
        public OutcomeFlowUseCases(IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor spExec, IItfCmdDataRepository cmdRepo, IItfCmdDataQueries cmdQueries, ISlotQueryRepository slotQueries)
            : this(warehouseRepo, spExec, cmdRepo, cmdQueries)
        {
            _slotQueries = slotQueries;
        }

        public async Task<IReadOnlyList<GradeClassBatteryDbDto>> SearchGradeWarehouseBatteriesAsync(
            string siteCode,
            string factoryCode,
            string gradeWarehouseCode,
            string labelSubstring,
            string selectedGrade,
            DateTime startCollectionDate,
            DateTime endCollectionDate,
            string carManufacture,
            string carModel,
            string batteryManufacture,
            string releaseYear,
            string batteryType,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (_slotQueries == null)
                {
                    AppLog.Warn("OutcomeFlowUseCases.SearchGradeWarehouseBatteriesAsync: ISlotQueryRepository not configured.");
                    return new List<GradeClassBatteryDbDto>();
                }

                // Delegate to repository implementation (A Query)
                var list = await _slotQueries.SearchGradeWarehouseBatteriesAsync(
                    siteCode,
                    factoryCode,
                    gradeWarehouseCode,
                    labelSubstring,
                    selectedGrade,
                    startCollectionDate,
                    endCollectionDate,
                    carManufacture,
                    carModel,
                    batteryManufacture,
                    releaseYear,
                    batteryType,
                    ct).ConfigureAwait(false);

                return list ?? new List<GradeClassBatteryDbDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.SearchGradeWarehouseBatteriesAsync failed.", ex);
                throw;
            }
        }

        private static int CompareDateString(string yyyymmdd, DateTime dt)
        {
            // Safely compare YYYYMMDD string with DateTime
            if (string.IsNullOrEmpty(yyyymmdd)) return 0;
            if (yyyymmdd.Length != 8) return 0;
            // Build comparable int
            int strVal;
            if (!int.TryParse(yyyymmdd, out strVal)) return 0;
            int dtVal = dt.Year * 10000 + dt.Month * 100 + dt.Day;
            if (strVal == dtVal) return 0;
            return strVal < dtVal ? -1 : 1;
        }

        // Moved from OutcomeFlowsUseCases.cs
        public async Task<bool> ProcessOutcomeToLoadingAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new InterfaceCommandUseCases(_repo, _queries, _sp)
                    .WaitForAndAcknowledgeAsync(GbtpLib.Mssql.Domain.EIfCmd.AA3, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.ProcessOutcomeToLoadingAsync failed.", ex);
                throw;
            }
        }

        // Moved from OutcomeFlowsUseCases.cs
        public async Task<bool> ProcessDefectToLoadingCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ok = await new InterfaceCommandUseCases(_repo, _queries, _sp)
                    .WaitForAndAcknowledgeAsync(GbtpLib.Mssql.Domain.EIfCmd.EE8, label, ct).ConfigureAwait(false);
                return ok;
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.ProcessDefectToLoadingCompletedAsync failed.", ex);
                throw;
            }
        }
    }
}
