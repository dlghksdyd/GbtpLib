using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class InterfaceCommandUseCases
    {
        private readonly IItfCmdDataRepository _repo;
        private readonly IItfCmdDataQueries _queries;
        private readonly IStoredProcedureExecutor _sp;

        public InterfaceCommandUseCases(
            IItfCmdDataRepository repo,
            IItfCmdDataQueries queries,
            IStoredProcedureExecutor sp)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        // Common SP execution utilities
        private async Task<bool> ExecuteCommandAsync(EIfCmd cmd, IDictionary<string, object> parameters, CancellationToken ct)
        {
            try
            {
                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
                parameters["@IN_CMD_CD"] = cmd.ToString();
                var result = await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.ExecuteCommandAsync failed for {cmd}.", ex);
                throw;
            }
        }

        private static IDictionary<string, object> BuildParams(
            string data1 = null, string data2 = null, string data3 = null, string data4 = null,
            string data5 = null, string data6 = null, string data7 = null, string data8 = null,
            string data9 = null, string data10 = null, string reqSys = null)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (data1 != null) dict["@IN_DATA1"] = data1;
            if (data2 != null) dict["@IN_DATA2"] = data2;
            if (data3 != null) dict["@IN_DATA3"] = data3;
            if (data4 != null) dict["@IN_DATA4"] = data4;
            if (data5 != null) dict["@IN_DATA5"] = data5;
            if (data6 != null) dict["@IN_DATA6"] = data6;
            if (data7 != null) dict["@IN_DATA7"] = data7;
            if (data8 != null) dict["@IN_DATA8"] = data8;
            if (data9 != null) dict["@IN_DATA9"] = data9;
            if (data10 != null) dict["@IN_DATA10"] = data10;
            if (reqSys != null) dict["@IN_REQ_SYS"] = reqSys;
            return dict;
        }

        // Send (enqueue) operations
        public Task<bool> SendOutcomeToWaitAsync(string label, string row, string col, string lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label, row, col, lvl, reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.EE1, p, ct);
        }

        public Task<bool> SendOutcomeAcceptToLoadingAsync(string label, string loadingRow, string loadingCol, string loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label, loadingRow, loadingCol, loadingLvl, reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.EE3, p, ct);
        }

        public Task<bool> SendOutcomeRejectToWaitAsync(string label, string outcomeWaitRow, string outcomeWaitCol, string outcomeWaitLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label, outcomeWaitRow, outcomeWaitCol, outcomeWaitLvl, reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.EE5, p, ct);
        }

        public Task<bool> SendDefectToLoadingAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label,
                defectRow.ToString(), defectCol.ToString(), defectLvl.ToString(),
                loadingRow.ToString(), loadingCol.ToString(), loadingLvl.ToString(),
                reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.EE7, p, ct);
        }

        public Task<bool> SendIncomeAcceptAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.AA2, p, ct);
        }

        public Task<bool> SendIncomeRejectAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.AA4, p, ct);
        }

        public Task<bool> SendMoveToIncomeWaitAsync(int fromRow, int fromCol, int fromLvl, int toRow, int toCol, int toLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            var p = BuildParams(string.Empty,
                fromRow.ToString(), fromCol.ToString(), fromLvl.ToString(),
                toRow.ToString(), toCol.ToString(), toLvl.ToString(),
                reqSys: reqSys);
            return ExecuteCommandAsync(EIfCmd.AA0, p, ct);
        }

        // Acknowledge and polling
        public async Task<bool> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.AcknowledgeAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> WaitAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
                if (list.Count == 0) { return false; }
                var affected = await _repo.AcknowledgeAsync(cmdToWait, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.WaitAndAcknowledgeAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> WaitAndAcknowledgeByCmdAsync(EIfCmd cmdToWait, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var list = await _queries.GetPendingByCmdAsync(cmdToWait.ToString(), ct).ConfigureAwait(false);
                if (list.Count == 0) return false;

                var oldest = list.OrderBy(x => x.RequestTime).FirstOrDefault();
                if (oldest == null) return false;

                var affected = await _repo.AcknowledgeAsync(cmdToWait, oldest.Data1 ?? string.Empty, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.WaitAndAcknowledgeByCmdAsync failed.", ex);
                throw;
            }
        }

        public Task<bool> WaitAndAcknowledgeIncomeCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            return WaitAndAcknowledgeAsync(EIfCmd.AA3, label, ct);
        }
    }
}
