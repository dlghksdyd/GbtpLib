using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

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
        public OutcomeFlowUseCases(IInvWarehouseRepository warehouseRepo, IStoredProcedureExecutor spExec, IItfCmdDataRepository cmdRepo, IItfCmdDataQueries cmdQueries)
        {
            _warehouseRepo = warehouseRepo;
            _sp = spExec;
            _repo = cmdRepo;
            _queries = cmdQueries;
        }

        public class GradeClassBatteryDbDto
        {
            public int ROW { get; set; }
            public int COL { get; set; }
            public int LVL { get; set; }
            public string LBL_ID { get; set; }
            public string INSP_GRD { get; set; }
            public string SITE_NM { get; set; }
            public string COLT_DAT { get; set; }
            public string COLT_RESN { get; set; }
            public string PACK_MDLE_CD { get; set; }
            public string BTR_TYPE_NM { get; set; }
            public string CAR_RELS_YEAR { get; set; }
            public string CAR_MAKE_NM { get; set; }
            public string CAR_NM { get; set; }
            public string BTR_MAKE_NM { get; set; }
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
                // TODO: Use _warehouseRepo (or dedicated query interface) to implement search
                await Task.Yield();
                return new List<GradeClassBatteryDbDto>();
            }
            catch (Exception ex)
            {
                AppLog.Error("OutcomeFlowUseCases.SearchGradeWarehouseBatteriesAsync failed.", ex);
                throw;
            }
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
