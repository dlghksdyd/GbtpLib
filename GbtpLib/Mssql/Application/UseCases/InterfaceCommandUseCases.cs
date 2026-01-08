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
        private readonly IItfCmdDataCommands _repo;
        private readonly IItfCmdDataQueries _queries;
        private readonly IStoredProcedureExecutor _sp;

        public InterfaceCommandUseCases(
            IItfCmdDataCommands repo,
            IItfCmdDataQueries queries,
            IStoredProcedureExecutor sp)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        private async Task<bool> ExecuteCommandAsync(EIfCmd cmd, IDictionary<string, object> parameters, CancellationToken ct)
        {
            try
            {
                if (parameters == null) throw new ArgumentNullException(nameof(parameters));
                parameters["@IN_CMD_CD"] = cmd.ToString();
                var result = await _sp.ExecuteAsync("BRDS_ITF_CMD_DATA_SET", parameters, ct).ConfigureAwait(false);
                // After enqueue attempt, verify existence of the enqueued record
                // Use DATA1 as the key paired with command code to check ITF_CMD_DATA presence
                parameters.TryGetValue("@IN_DATA1", out var data1Obj);
                var data1 = data1Obj as string ?? string.Empty;
                var list = await _queries.GetPendingAsync(cmd.ToString(), data1, ct).ConfigureAwait(false);
                return list.Count > 0;
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

        public Task<bool> SendOutcomeToWaitAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.EE1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendOutcomeToWaitAsync failed. label={label}, row={row}, col={col}, lvl={lvl}", ex);
                throw;
            }
        }

        public Task<bool> SendOutcomeAcceptToLoadingAsync(string label, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label, loadingRow.ToString(), loadingCol.ToString(), loadingLvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.EE3, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendOutcomeAcceptToLoadingAsync failed. label={label}, row={loadingRow}, col={loadingCol}, lvl={loadingLvl}", ex);
                throw;
            }
        }

        public Task<bool> SendOutcomeRejectToWaitAsync(string label, int outcomeWaitRow, int outcomeWaitCol, int outcomeWaitLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label, outcomeWaitRow.ToString(), outcomeWaitCol.ToString(), outcomeWaitLvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.EE5, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendOutcomeRejectToWaitAsync failed. label={label}, row={outcomeWaitRow}, col={outcomeWaitCol}, lvl={outcomeWaitLvl}", ex);
                throw;
            }
        }

        public Task<bool> SendDefectToLoadingAsync(string label, int defectRow, int defectCol, int defectLvl, int loadingRow, int loadingCol, int loadingLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label,
                    defectRow.ToString(), defectCol.ToString(), defectLvl.ToString(),
                    loadingRow.ToString(), loadingCol.ToString(), loadingLvl.ToString(),
                    reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.EE7, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDefectToLoadingAsync failed. label={label}, defectRow={defectRow}, defectCol={defectCol}, defectLvl={defectLvl}, loadingRow={loadingRow}, loadingCol={loadingCol}, loadingLvl={loadingLvl}", ex);
                throw;
            }
        }

        public Task<bool> SendIncomeAcceptAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.AA2, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendIncomeAcceptAsync failed. label={label}, row={row}, col={col}, lvl={lvl}", ex);
                throw;
            }
        }

        public Task<bool> SendIncomeRejectAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.AA4, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendIncomeRejectAsync failed. label={label}, row={row}, col={col}, lvl={lvl}", ex);
                throw;
            }
        }

        // AA1 입고요청
        // DATA1=배터리라벨ID, DATA2/3/4=입고대 위치(행/열/단)
        public Task<bool> SendIncomeRequestAsync(string label, int row, int col, int lvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (row < 0 || col < 0 || lvl < 0) throw new ArgumentOutOfRangeException("coordinates must be non-negative");

                var p = BuildParams(label, row.ToString(), col.ToString(), lvl.ToString(), reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.AA1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendIncomeRequestAsync failed. label={label}, row={row}, col={col}, lvl={lvl}", ex);
                throw;
            }
        }

        // AA5 입고완료 확인
        // DATA1=배터리라벨ID
        public Task<bool> SendIncomeCompletedAckAsync(string label, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                var p = BuildParams(data1: label, reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.AA5, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendIncomeCompletedAckAsync failed. label={label}", ex);
                throw;
            }
        }

        public Task<bool> SendMoveToIncomeWaitAsync(int fromRow, int fromCol, int fromLvl, int toRow, int toCol, int toLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var p = BuildParams(string.Empty,
                    fromRow.ToString(), fromCol.ToString(), fromLvl.ToString(),
                    toRow.ToString(), toCol.ToString(), toLvl.ToString(),
                    reqSys: reqSys);
                return ExecuteCommandAsync(EIfCmd.AA0, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendMoveToIncomeWaitAsync failed. fromRow={fromRow}, fromCol={fromCol}, fromLvl={fromLvl}, toRow={toRow}, toCol={toCol}, toLvl={toLvl}", ex);
                throw;
            }
        }

        // BB0 작업 시작 정보전송
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=작업구분(1:활성화, 2:진단), DATA5=작업유형(0:Remote Manual, 1:Remote Schedule)
        public Task<bool> SendActivationStartInfoAsync(string label, string equipmentCode, string channelId, int jobCategory, int jobType, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (jobCategory != 1 && jobCategory != 2) throw new ArgumentOutOfRangeException(nameof(jobCategory), "jobCategory must be 1 or 2");
                if (jobType != 0 && jobType != 1) throw new ArgumentOutOfRangeException(nameof(jobType), "jobType must be 0 or 1");

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: jobCategory.ToString(),
                    data5: jobType.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB0, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendActivationStartInfoAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, jobCategory={jobCategory}, jobType={jobType}", ex);
                throw;
            }
        }

        // BB1 배터리 이송요청
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=작업구분(1:활성화, 2:진단)
        public Task<bool> SendBatteryTransferRequestAsync(string label, string equipmentCode, string channelId, int jobCategory, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (jobCategory != 1 && jobCategory != 2) throw new ArgumentOutOfRangeException(nameof(jobCategory), "jobCategory must be 1 or 2");

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: jobCategory.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendBatteryTransferRequestAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, jobCategory={jobCategory}", ex);
                throw;
            }
        }

        // BB2 배터리 이송완료
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendBatteryTransferCompletedAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB2, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendBatteryTransferCompletedAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB3 체결 시작
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=커넥터타입
        public Task<bool> SendFastenStartAsync(string label, string equipmentCode, string channelId, string connectorType, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (string.IsNullOrWhiteSpace(connectorType)) throw new ArgumentException("connectorType is required", nameof(connectorType));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: connectorType,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB3, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendFastenStartAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, connectorType={connectorType}", ex);
                throw;
            }
        }

        // BB4 체결 완료
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendFastenCompletedAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB4, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendFastenCompletedAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB5 체결 완료 확인
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendFastenCompletedAckAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB5, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendFastenCompletedAckAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB6 체결 해제 요청
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=커넥터타입
        public Task<bool> SendUnfastenRequestAsync(string label, string equipmentCode, string channelId, string connectorType, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (string.IsNullOrWhiteSpace(connectorType)) throw new ArgumentException("connectorType is required", nameof(connectorType));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: connectorType,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB6, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendUnfastenRequestAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, connectorType={connectorType}", ex);
                throw;
            }
        }

        // BB7 체결 해제 완료
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendUnfastenCompletedAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB7, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendUnfastenCompletedAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB8 체결 해제 완료 확인
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendUnfastenCompletedAckAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB8, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendUnfastenCompletedAckAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB9 재체결 시작
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=커넥터타입
        public Task<bool> SendRefastenStartAsync(string label, string equipmentCode, string channelId, string connectorType, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (string.IsNullOrWhiteSpace(connectorType)) throw new ArgumentException("connectorType is required", nameof(connectorType));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: connectorType,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB9, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendRefastenStartAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, connectorType={connectorType}", ex);
                throw;
            }
        }

        // BB10 재체결 완료
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendRefastenCompletedAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB10, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendRefastenCompletedAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // BB11 재체결 완료 확인(자동체결로봇 이동허가)
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendRefastenCompletedAckAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.BB11, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendRefastenCompletedAckAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
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
                AppLog.Error($"InterfaceCommandUseCases.AcknowledgeAsync failed. cmd={cmd}, data1={data1}", ex);
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
                AppLog.Error($"InterfaceCommandUseCases.WaitAndAcknowledgeAsync failed. cmd={cmdToWait}, data1={data1}", ex);
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
                AppLog.Error($"InterfaceCommandUseCases.WaitAndAcknowledgeByCmdAsync failed. cmd={cmdToWait}", ex);
                throw;
            }
        }

        public Task<bool> WaitAndAcknowledgeIncomeCompletedAsync(string label, CancellationToken ct = default(CancellationToken))
        {
            return WaitAndAcknowledgeAsync(EIfCmd.AA3, label, ct);
        }

        // CC1 재입고 요청
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendReincomingRequestAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.CC1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendReincomingRequestAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // CC2 재입고 완료
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID
        public Task<bool> SendReincomingCompletedAsync(string label, string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.CC2, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendReincomingCompletedAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }

        // DD1 출고레일 이송 요청 (정상 검사 종료)
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=등급분류코드(1~4)
        public Task<bool> SendDiagnosisToRailRequestAsync(string label, string equipmentCode, string channelId, int gradeCode, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (gradeCode < 1 || gradeCode > 4) throw new ArgumentOutOfRangeException(nameof(gradeCode), "gradeCode must be between 1 and 4");

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: gradeCode.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.DD1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDiagnosisToRailRequestAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, gradeCode={gradeCode}", ex);
                throw;
            }
        }

        // DD2 등급분류 적재창고 적재 완료
        // DATA1=배터리라벨ID
        public Task<bool> SendDiagnosisWarehouseLoadCompletedAsync(string label, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));

                var p = BuildParams(
                    data1: label,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.DD2, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDiagnosisWarehouseLoadCompletedAsync failed. label={label}", ex);
                throw;
            }
        }

        // FF1 불량적재 요청 (검사 중 비정상 종료)
        // DATA1=배터리라벨ID, DATA2=설비코드, DATA3=채널ID, DATA4=에러코드, DATA5/6/7=불량적재 창고 위치(행/열/단)
        public Task<bool> SendDefectStorageRequestAsync(string label, string equipmentCode, string channelId, string errorCode,
            int defectRow, int defectCol, int defectLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));
                if (string.IsNullOrWhiteSpace(errorCode)) throw new ArgumentException("errorCode is required", nameof(errorCode));
                if (defectRow < 0 || defectCol < 0 || defectLvl < 0) throw new ArgumentOutOfRangeException("defect coordinates must be non-negative");

                var p = BuildParams(
                    data1: label,
                    data2: equipmentCode,
                    data3: channelId,
                    data4: errorCode,
                    data5: defectRow.ToString(),
                    data6: defectCol.ToString(),
                    data7: defectLvl.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.FF1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDefectStorageRequestAsync failed. label={label}, equipmentCode={equipmentCode}, channelId={channelId}, errorCode={errorCode}, defectRow={defectRow}, defectCol={defectCol}, defectLvl={defectLvl}", ex);
                throw;
            }
        }

        // FF2 불량적재 창고장소 적재완료
        // DATA1=배터리라벨ID, DATA2/3/4=불량창고 위치(행/열/단)
        public Task<bool> SendDefectStorageCompletedAsync(string label, int defectRow, int defectCol, int defectLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (defectRow < 0 || defectCol < 0 || defectLvl < 0) throw new ArgumentOutOfRangeException("defect coordinates must be non-negative");

                var p = BuildParams(
                    data1: label,
                    data2: defectRow.ToString(),
                    data3: defectCol.ToString(),
                    data4: defectLvl.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.FF2, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDefectStorageCompletedAsync failed. label={label}, defectRow={defectRow}, defectCol={defectCol}, defectLvl={defectLvl}", ex);
                throw;
            }
        }

        // GG1 불량적재 창고 재입고 요청
        // DATA1=배터리라벨ID, DATA2/3/4=불량창고 위치(행/열/단)
        public Task<bool> SendDefectReincomingRequestAsync(string label, int defectRow, int defectCol, int defectLvl, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("label is required", nameof(label));
                if (defectRow < 0 || defectCol < 0 || defectLvl < 0) throw new ArgumentOutOfRangeException("defect coordinates must be non-negative");

                var p = BuildParams(
                    data1: label,
                    data2: defectRow.ToString(),
                    data3: defectCol.ToString(),
                    data4: defectLvl.ToString(),
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.GG1, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendDefectReincomingRequestAsync failed. label={label}, defectRow={defectRow}, defectCol={defectCol}, defectLvl={defectLvl}", ex);
                throw;
            }
        }

        // HH3 전체 시스템 비상정지
        // DATA1=시스템코드(진단/활성화는 설비코드)
        public Task<bool> SendEmergencyStopAsync(string systemCodeOrEquipment, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(systemCodeOrEquipment)) throw new ArgumentException("systemCodeOrEquipment is required", nameof(systemCodeOrEquipment));

                var p = BuildParams(
                    data1: systemCodeOrEquipment,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.HH3, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendEmergencyStopAsync failed. systemCodeOrEquipment={systemCodeOrEquipment}", ex);
                throw;
            }
        }

        // HH4 전체 시스템 재가동 시작
        // DATA1=시스템코드(진단/활성화는 설비코드)
        public Task<bool> SendSystemRerunStartAsync(string systemCodeOrEquipment, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(systemCodeOrEquipment)) throw new ArgumentException("systemCodeOrEquipment is required", nameof(systemCodeOrEquipment));

                var p = BuildParams(
                    data1: systemCodeOrEquipment,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.HH4, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendSystemRerunStartAsync failed. systemCodeOrEquipment={systemCodeOrEquipment}", ex);
                throw;
            }
        }

        // HH5 전체 시스템 비상정지 및 긴급위험
        // DATA1=설비코드, DATA2=채널ID
        public Task<bool> SendEmergencyStopAndDangerAsync(string equipmentCode, string channelId, string reqSys, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(equipmentCode)) throw new ArgumentException("equipmentCode is required", nameof(equipmentCode));
                if (string.IsNullOrWhiteSpace(channelId)) throw new ArgumentException("channelId is required", nameof(channelId));

                var p = BuildParams(
                    data1: equipmentCode,
                    data2: channelId,
                    reqSys: reqSys);

                return ExecuteCommandAsync(EIfCmd.HH5, p, ct);
            }
            catch (Exception ex)
            {
                AppLog.Error($"InterfaceCommandUseCases.SendEmergencyStopAndDangerAsync failed. equipmentCode={equipmentCode}, channelId={channelId}", ex);
                throw;
            }
        }
    }
}
