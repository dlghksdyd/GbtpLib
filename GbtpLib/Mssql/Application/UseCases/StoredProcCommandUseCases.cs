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
    /// Centralized use cases that call stored procedure BRDS_ITF_CMD_DATA_SET.
    /// Returns true when no exception occurs, false when an exception is thrown.
    /// </summary>
    public class StoredProcCommandUseCases
    {
        private readonly IStoredProcedureExecutor _sp;

        public StoredProcCommandUseCases(IStoredProcedureExecutor sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        public async Task<bool> RequestOutcomeWaitAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@IN_CMD_CD", EIfCmd.EE1.ToString()},
                    {"@IN_DATA1", label},
                    {"@IN_DATA2", row},
                    {"@IN_DATA3", col},
                    {"@IN_DATA4", lvl},
                    {"@IN_REQ_SYS", reqSys},
                };
                await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("StoredProcCommandUseCases.RequestOutcomeWaitAsync failed.", ex);
                return false;
            }
        }

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
                await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("StoredProcCommandUseCases.RequestAcceptAsync failed.", ex);
                return false;
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
                await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("StoredProcCommandUseCases.RequestRejectAsync failed.", ex);
                return false;
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
                await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("StoredProcCommandUseCases.RequestDefectToLoadingAsync failed.", ex);
                return false;
            }
        }

        public async Task<bool> RequestIncomeMoveAsync(int unloadingRow, int unloadingCol, int unloadingLvl, int incomeRow, int incomeCol, int incomeLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@IN_CMD_CD", EIfCmd.AA0.ToString()},
                    {"@IN_DATA1", string.Empty},
                    {"@IN_DATA2", unloadingRow},
                    {"@IN_DATA3", unloadingCol},
                    {"@IN_DATA4", unloadingLvl},
                    {"@IN_DATA5", incomeRow},
                    {"@IN_DATA6", incomeCol},
                    {"@IN_DATA7", incomeLvl},
                    {"@IN_REQ_SYS", reqSys},
                };
                await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("StoredProcCommandUseCases.RequestIncomeMoveAsync failed.", ex);
                return false;
            }
        }
    }
}
