using System;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Application.UseCases;
using GbtpLib.Mssql.Configuration;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Db;
using GbtpLib.Mssql.Persistence.Repositories;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.Services
{
    // 단일 진입점: Db/UoW + 모든 Repo/Query/UseCase를 손쉽게 사용하도록 제공
    public static class ServiceFactory
    {
        // 애플리케이션 시작 시 1회 호출하여 연결 문자열 설정 보장
        public static void Initialize(string connectionString)
        {
            DbConnectionSettings.Initialize(connectionString);
        }

        // 한 번에 자주 쓰는 의존성들을 묶어서 제공
        public sealed class Services : IDisposable
        {
            // 내부 관리용 컨텍스트/단위작업
            private readonly IAppDbContext _db;
            private readonly IUnitOfWork _uow;

            // 내부 관리용 Repositories / Queries
            private readonly IMstUserInfoRepository _users;
            private readonly IInvWarehouseRepository _warehouses;
            private readonly IItfCmdDataRepository _cmdRepo;
            private readonly IItfCmdDataQueries _cmdQueries;
            private readonly IStoredProcedureExecutor _storedProc;
            private readonly IMstCodeRepository _codes;
            private readonly ISlotQueryRepository _slots;
            private readonly IMstBtrRepository _batteries;
            private readonly IMstBtrTypeRepository _batteryTypes;
            private readonly IMetadataQueries _metadata;
            private readonly ILabelCreationQueries _labelCreation;
            private readonly IQltBtrInspQueries _inspection;
            private readonly IDefectBatteryQueries _defects;
            private readonly IWarehouseQueries _warehouseQueries;
            private readonly IWarehouseLayoutQueries _warehouseLayout;

            // 공개 UseCases
            public LoginUseCase Login { get; }
            public GetCodeUseCase GetCode { get; }
            public MetadataUseCases MetadataUseCases { get; }
            public InitializeSlotsUseCase InitializeSlots { get; }
            public UpdateWarehouseSlotUseCase UpdateWarehouseSlot { get; }
            public EnqueueCommandUseCase EnqueueCommand { get; }
            public AcknowledgeCommandUseCase AcknowledgeCommand { get; }
            public RequestTransferUseCase RequestTransfer { get; }
            public CommandPollingUseCase CommandPolling { get; }
            public CreateLabelUseCase CreateLabel { get; }
            public DeleteLabelFlowUseCase DeleteLabelFlow { get; }
            public LabelCreationMetadataUseCase LabelCreationMetadata { get; }
            public LabelCreationUseCase LabelCreationUseCase { get; }
            public GradeLookupUseCase GradeLookup { get; }
            public CreateLabelAndAssignSlotUseCase CreateLabelAndAssignSlot { get; }
            public OutcomeFlowUseCases OutcomeFlow { get; }
            public IncomeFlowUseCases IncomeFlow { get; }
            public FilterMetadataUseCase FilterMetadata { get; }

            internal Services(IAppDbContext db, IUnitOfWork uow)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                _uow = uow ?? throw new ArgumentNullException(nameof(uow));

                // repos/queries
                _users = new MstUserInfoRepository(_db);
                _warehouses = new InvWarehouseRepository(_db);
                _cmdRepo = new ItfCmdDataRepository(_db);
                _cmdQueries = new ItfCmdDataQueries(_db);
                _storedProc = new StoredProcedureExecutor(_db);
                _codes = new MstCodeRepository(_db);
                _slots = new SlotQueryRepository(_db);
                _batteries = new MstBtrRepository(_db);
                _batteryTypes = new MstBtrTypeRepository(_db);
                _metadata = new MetadataQueries(_db);
                _labelCreation = new LabelCreationQueries(_db);
                _inspection = new QltBtrInspQueries(_db);
                _defects = new DefectBatteryQueries(_db);
                _warehouseQueries = new WarehouseQueries(_db);
                _warehouseLayout = new WarehouseLayoutQueries(_db);

                // usecases
                Login = new LoginUseCase(_uow, _users);
                GetCode = new GetCodeUseCase(_uow, _codes);
                MetadataUseCases = new MetadataUseCases(_uow, _metadata);
                InitializeSlots = new InitializeSlotsUseCase(_uow, _slots);
                UpdateWarehouseSlot = new UpdateWarehouseSlotUseCase(_uow, _warehouses);
                EnqueueCommand = new EnqueueCommandUseCase(_uow, _cmdRepo);
                AcknowledgeCommand = new AcknowledgeCommandUseCase(_uow, _cmdRepo);
                RequestTransfer = new RequestTransferUseCase(_uow, _storedProc);
                CommandPolling = new CommandPollingUseCase(_uow, _cmdQueries, _cmdRepo);
                CreateLabel = new CreateLabelUseCase(_uow, _batteries);
                DeleteLabelFlow = new DeleteLabelFlowUseCase(_uow, _batteries, _warehouses);
                LabelCreationMetadata = new LabelCreationMetadataUseCase(_uow, _labelCreation);
                LabelCreationUseCase = new LabelCreationUseCase(_uow, _batteryTypes, _batteries);
                GradeLookup = new GradeLookupUseCase(_uow, _inspection);
                CreateLabelAndAssignSlot = new CreateLabelAndAssignSlotUseCase(_uow, _batteries, _warehouses);
                OutcomeFlow = new OutcomeFlowUseCases(_uow, _warehouses, _storedProc, _cmdRepo, _cmdQueries);
                IncomeFlow = new IncomeFlowUseCases(_uow, _storedProc, _cmdRepo, _cmdQueries, _warehouses);
                FilterMetadata = new FilterMetadataUseCase(_uow, _metadata);
            }

            // 트랜잭션 경계 제어를 위한 위임 메서드들
            public void Begin() { _uow.Begin(); }
            public void Commit() { _uow.Commit(); }
            public void Rollback() { _uow.Rollback(); }
            public Task BeginAsync() { return _uow.BeginAsync(); }
            public Task CommitAsync() { return _uow.CommitAsync(); }
            public Task RollbackAsync() { return _uow.RollbackAsync(); }

            public void Dispose()
            {
                (_db as IDisposable)?.Dispose();
            }
        }

        // 기본 연결 문자열로 Services 생성
        public static Services CreateServices()
        {
            var factory = new AppDbContextFactory(DbConnectionSettings.ConnectionString);
            var db = factory.Create();
            var uow = new GbtpLib.Mssql.Persistence.UnitOfWork.EfUnitOfWork(db);
            return new Services(db, uow);
        }

        /// <summary>
        /// using 패턴으로 안전 사용하며 성공 여부를 반환합니다.
        /// true: 액션이 예외 없이 완료됨, false: 실행 중 예외 발생.
        /// </summary>
        /// <param name="action">Services를 받아 작업을 수행하는 동기 액션</param>
        /// <returns>성공 여부(bool)</returns>
        /// <remarks>
        /// 사용 예:
        ///
        /// // 연결 문자열 설정 (앱 시작 시 1회)
        /// ServiceFactory.Initialize(connString);
        ///
        /// // 동기 사용
        /// var ok = ServiceFactory.UsingServices(s =>
        /// {
        ///     // 예: 라벨 생성
        ///     var req = new CreateLabelUseCase.Request { /* ... */ };
        ///     var res = s.CreateLabel.Execute(req);
        ///
        ///     // 예: 코드 조회
        ///     var code = s.GetCode.Execute("CATEGORY", "KEY");
        ///
        ///     // 예: 등급 조회 (GradeLookupUseCase)
        ///     var gradeReq = new GradeLookupUseCase.Request { /* 배터리 정보 등 입력 */ };
        ///     var gradeRes = s.GradeLookup.Execute(gradeReq);
        /// });
        ///
        /// if (!ok)
        /// {
        ///     // 실패 처리 로직
        /// }
        /// </remarks>
        public static bool UsingServices(Action<Services> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            using (var services = CreateServices())
            {
                try
                {
                    services.Begin();
                    action(services);
                    services.Commit();
                    return true;
                }
                catch
                {
                    try { services.Rollback(); } catch { /* swallow rollback errors */ }
                    return false;
                }
            }
        }

        /// <summary>
        /// using 패턴으로 안전한 비동기 사용을 제공하며 성공 여부를 반환합니다.
        /// true: 액션이 예외 없이 완료됨, false: 실행 중 예외 발생.
        /// </summary>
        /// <param name="action">Services를 받아 Task를 반환하는 비동기 함수</param>
        /// <returns>성공 여부(Task&lt;bool&gt;)</returns>
        /// <remarks>
        /// 사용 예:
        ///
        /// // 연결 문자열 설정 (앱 시작 시 1회)
        /// ServiceFactory.Initialize(connString);
        ///
        /// // 비동기 사용
        /// var ok = await ServiceFactory.UsingServicesAsync(async s =>
        /// {
        ///     // 예: 명령 큐에 등록
        ///     await s.EnqueueCommand.ExecuteAsync(new EnqueueCommandUseCase.Request { /* ... */ });
        ///
        ///     // 예: 창고 슬롯 업데이트
        ///     await s.UpdateWarehouseSlot.ExecuteAsync(new UpdateWarehouseSlotUseCase.Request { /* ... */ });
        ///
        ///     // 예: 등급 조회 (GradeLookupUseCase)
        ///     var gradeReq = new GradeLookupUseCase.Request { /* 배터리 정보 등 입력 */ };
        ///     var gradeRes = s.GradeLookup.Execute(gradeReq);
        /// });
        ///
        /// if (!ok)
        /// {
        ///     // 실패 처리 로직
        /// }
        /// </remarks>
        public static async Task<bool> UsingServicesAsync(Func<Services, Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            using (var services = CreateServices())
            {
                try
                {
                    await services.BeginAsync().ConfigureAwait(false);
                    await action(services).ConfigureAwait(false);
                    await services.CommitAsync().ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    try { await services.RollbackAsync().ConfigureAwait(false); } catch { /* swallow rollback errors */ }
                    return false;
                }
            }
        }
    }
}
