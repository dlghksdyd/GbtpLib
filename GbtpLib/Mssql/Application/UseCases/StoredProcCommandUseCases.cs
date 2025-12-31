using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;
using System.Diagnostics;
using System.Text;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Centralized use cases that call stored procedure BRDS_ITF_CMD_DATA_SET.
    /// Legacy bool methods are kept; richer Result-returning methods are also provided.
    /// </summary>
    public class StoredProcCommandUseCases
    {
        private const string SpName = "BRDS_ITF_CMD_DATA_SET";
        private readonly IStoredProcedureExecutor _sp;

        public StoredProcCommandUseCases(IStoredProcedureExecutor sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        // =====================
        // Legacy bool-returning APIs (kept for compatibility)
        // =====================
        public Task<bool> RequestOutcomeWaitAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendAsync(EIfCmd.EE1, reqSys, ct, label, row, col, lvl);
        }

        public Task<bool> RequestAcceptAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendAsync(EIfCmd.AA2, reqSys, ct, label, row, col, lvl);
        }

        public Task<bool> RequestRejectAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendAsync(EIfCmd.AA4, reqSys, ct, label, row, col, lvl);
        }

        public Task<bool> RequestDefectToLoadingAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendAsync(EIfCmd.EE7, reqSys, ct, label, defectRow, defectCol, defectLvl, loadingRow, loadingCol, loadingLvl);
        }

        public Task<bool> RequestIncomeMoveAsync(int unloadingRow, int unloadingCol, int unloadingLvl, int incomeRow, int incomeCol, int incomeLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            // Income move does not include a label (DATA1 is empty)
            return SendAsync(EIfCmd.AA0, reqSys, ct, string.Empty, unloadingRow, unloadingCol, unloadingLvl, incomeRow, incomeCol, incomeLvl);
        }

        // =====================
        // Preferred Result-returning APIs
        // =====================
        public Task<StoredProcCommandResult> RequestOutcomeWaitResultAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendWithResultAsync(EIfCmd.EE1, reqSys, ct, label, row, col, lvl);
        }

        public Task<StoredProcCommandResult> RequestAcceptResultAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendWithResultAsync(EIfCmd.AA2, reqSys, ct, label, row, col, lvl);
        }

        public Task<StoredProcCommandResult> RequestRejectResultAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendWithResultAsync(EIfCmd.AA4, reqSys, ct, label, row, col, lvl);
        }

        public Task<StoredProcCommandResult> RequestDefectToLoadingResultAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendWithResultAsync(EIfCmd.EE7, reqSys, ct, label, defectRow, defectCol, defectLvl, loadingRow, loadingCol, loadingLvl);
        }

        public Task<StoredProcCommandResult> RequestIncomeMoveResultAsync(int unloadingRow, int unloadingCol, int unloadingLvl, int incomeRow, int incomeCol, int incomeLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            return SendWithResultAsync(EIfCmd.AA0, reqSys, ct, string.Empty, unloadingRow, unloadingCol, unloadingLvl, incomeRow, incomeCol, incomeLvl);
        }

        // =====================
        // Private helpers
        // =====================
        private async Task<bool> SendAsync(EIfCmd cmd, string reqSys, CancellationToken ct, params object[] data)
        {
            var result = await SendWithResultAsync(cmd, reqSys, ct, data).ConfigureAwait(false);
            return result.Success;
        }

        private async Task<StoredProcCommandResult> SendWithResultAsync(EIfCmd cmd, string reqSys, CancellationToken ct, params object[] data)
        {
            IDictionary<string, object> parameters = null;
            var sw = Stopwatch.StartNew();
            try
            {
                ValidateInputs(cmd, reqSys, data);
                parameters = BuildParameters(cmd, reqSys, data);
                AppLog.Trace($"SP start {SpName} cmd={cmd}, reqSys={reqSys}, data={FormatDataForLog(data)}");
                var affected = await _sp.ExecuteAsync(SpName, parameters, ct).ConfigureAwait(false);
                sw.Stop();
                AppLog.Info($"SP done {SpName} cmd={cmd}, reqSys={reqSys}, rows={affected}, elapsedMs={sw.ElapsedMilliseconds}");
                return StoredProcCommandResult.SuccessResult(cmd, reqSys, affected, parameters);
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppLog.Error($"SP error {SpName} cmd={cmd}, reqSys={reqSys}, elapsedMs={sw.ElapsedMilliseconds}, data={FormatDataForLog(data)}", ex);
                return StoredProcCommandResult.Failure(cmd, reqSys, ex, parameters);
            }
        }

        private static IDictionary<string, object> BuildParameters(EIfCmd cmd, string reqSys, object[] data)
        {
            var dict = new Dictionary<string, object>(2 + (data == null ? 0 : data.Length));
            dict["@IN_CMD_CD"] = cmd.ToString();
            dict["@IN_REQ_SYS"] = reqSys ?? string.Empty;

            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    dict["@IN_DATA" + (i + 1)] = data[i] ?? DBNull.Value;
                }
            }

            return dict;
        }

        private static void ValidateInputs(EIfCmd cmd, string reqSys, object[] data)
        {
            if (string.IsNullOrWhiteSpace(reqSys))
                throw new ArgumentException("reqSys must be provided.", nameof(reqSys));

            // Label validation: for most commands label is DATA1 (string). AA0 uses empty label intentionally.
            if (cmd != EIfCmd.AA0 && data != null && data.Length > 0 && data[0] is string)
            {
                var label = (string)data[0];
                if (string.IsNullOrWhiteSpace(label))
                    throw new ArgumentException("label must not be null or empty for this command.");
            }

            // Basic numeric range checks for coordinates/levels
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var v = data[i];
                    if (v is int)
                    {
                        if ((int)v < 0)
                            throw new ArgumentOutOfRangeException($"@IN_DATA{i + 1}", "Value must be >= 0.");
                    }
                }
            }
        }

        private static string FormatDataForLog(object[] data)
        {
            if (data == null || data.Length == 0) return "-";
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var v = data[i];
                if (v is string s)
                {
                    sb.Append($"d{i + 1}={Mask(s)}");
                }
                else
                {
                    sb.Append($"d{i + 1}={v}");
                }
            }
            return sb.ToString();
        }

        private static string Mask(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            // keep first 2 and last 2 chars, mask the rest
            if (s.Length <= 4) return new string('*', s.Length);
            return s.Substring(0, 2) + new string('*', s.Length - 4) + s.Substring(s.Length - 2, 2);
        }
    }

    /// <summary>
    /// Result object for stored-procedure-based interface commands.
    /// </summary>
    public sealed class StoredProcCommandResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Exception { get; private set; }
        public EIfCmd Command { get; private set; }
        public string RequestSystem { get; private set; }
        public int RowsAffected { get; private set; }
        public IDictionary<string, object> Parameters { get; private set; }

        private StoredProcCommandResult() { }

        public static StoredProcCommandResult SuccessResult(EIfCmd cmd, string reqSys, int affected, IDictionary<string, object> parameters)
        {
            return new StoredProcCommandResult
            {
                Success = true,
                ErrorMessage = null,
                Exception = null,
                Command = cmd,
                RequestSystem = reqSys,
                RowsAffected = affected,
                Parameters = parameters,
            };
        }

        public static StoredProcCommandResult Failure(EIfCmd cmd, string reqSys, Exception ex, IDictionary<string, object> parameters)
        {
            return new StoredProcCommandResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Exception = ex,
                Command = cmd,
                RequestSystem = reqSys,
                RowsAffected = 0,
                Parameters = parameters,
            };
        }
    }
}
