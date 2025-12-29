using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Aggregates interface command-related operations: enqueue, acknowledge, polling and SP-based requests.
    /// </summary>
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

        // Repository-based operations
        public async Task<bool> EnqueueAsync(EIfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.EnqueueAsync(cmd, data1, data2, data3, data4, requestSystem, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.EnqueueAsync failed.", ex);
                throw;
            }
        }

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

        public async Task<bool> WaitForAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
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
                AppLog.Error("InterfaceCommandUseCases.WaitForAndAcknowledgeAsync failed.", ex);
                throw;
            }
        }

        // Stored-procedure based operations
        public async Task<bool> RequestAcceptAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@IN_CMD_CD", EIfCmd.AA2.ToString()},
                    {"@IN_DATA1", label},
                    {"@IN_DATA2", row},
                    {"@IN_DATA3", col},
                    {"@IN_DATA4", lvl},
                    {"@IN_REQ_SYS", reqSys},
                };
                var result = await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.RequestAcceptAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> RequestRejectAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@IN_CMD_CD", EIfCmd.AA4.ToString()},
                    {"@IN_DATA1", label},
                    {"@IN_DATA2", row},
                    {"@IN_DATA3", col},
                    {"@IN_DATA4", lvl},
                    {"@IN_REQ_SYS", reqSys},
                };
                var result = await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.RequestRejectAsync failed.", ex);
                throw;
            }
        }

        public async Task<bool> RequestDefectToLoadingAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@IN_CMD_CD", EIfCmd.EE7.ToString()},
                    {"@IN_DATA1", label},
                    {"@IN_DATA2", defectRow},
                    {"@IN_DATA3", defectCol},
                    {"@IN_DATA4", defectLvl},
                    {"@IN_DATA5", loadingRow},
                    {"@IN_DATA6", loadingCol},
                    {"@IN_DATA7", loadingLvl},
                    {"@IN_REQ_SYS", reqSys},
                };
                var result = await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InterfaceCommandUseCases.RequestDefectToLoadingAsync failed.", ex);
                throw;
            }
        }
    }
}
