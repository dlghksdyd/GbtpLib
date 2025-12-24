using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Issues interface commands via stored procedures to mimic reference behavior.
    /// <para>
    /// Calls BRDS_ITF_CMD_DATA_SET with command and payload parameters.
    /// Return semantics:
    /// - <c>true</c>: Stored procedure executed successfully; the returned affected rows can be &gt;= 0.
    /// - <c>false</c>: Not used; exceptions indicate failure paths.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class RequestTransferUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IStoredProcedureExecutor _sp;
        public RequestTransferUseCase(IUnitOfWork uow, IStoredProcedureExecutor sp)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        /// <summary>
        /// Requests transfer acceptance (AA2) with position parameters.
        /// </summary>
        /// <param name="label">Label identifier.</param>
        /// <param name="row">Row index.</param>
        /// <param name="col">Column index.</param>
        /// <param name="lvl">Level index.</param>
        /// <param name="reqSys">Request system code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if SP executed (affected rows &gt;= 0).</returns>
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
                return result >= 0; // NonQuery returns affected rows (may be 0)
            }
            catch (Exception ex)
            {
                AppLog.Error("RequestTransferUseCase.RequestAcceptAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Requests transfer rejection (AA4) with position parameters.
        /// </summary>
        /// <returns><c>true</c> if SP executed (affected rows &gt;= 0).</returns>
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
                AppLog.Error("RequestTransferUseCase.RequestRejectAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Requests a defect-to-loading operation (EE7) providing both defect and loading positions.
        /// </summary>
        /// <returns><c>true</c> if SP executed (affected rows &gt;= 0).</returns>
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
                AppLog.Error("RequestTransferUseCase.RequestDefectToLoadingAsync failed.", ex);
                throw;
            }
        }
    }
}
