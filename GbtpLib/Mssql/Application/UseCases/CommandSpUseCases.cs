using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    // UseCases for SP-based interface commands to mimic Reference behavior
    public class RequestTransferUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IStoredProcedureExecutor _sp;
        public RequestTransferUseCase(IUnitOfWork uow, IStoredProcedureExecutor sp)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        public async Task<bool> RequestAcceptAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
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
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return result >= 0; // NonQuery returns affected rows (may be 0)
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<bool> RequestRejectAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
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
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<bool> RequestDefectToLoadingAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
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
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return result >= 0;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
