using System;
using System.Threading; // added for SynchronizationContext
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Application.UseCases;
using GbtpLib.Mssql.Configuration;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Db;
using GbtpLib.Mssql.Persistence.Repositories;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using System.Reflection; // for reflection on WPF Application/Dispatcher

namespace GbtpLib.Mssql.Application.Services
{
    /*
    Usage
    -----
    1) 애플리케이션 시작 시 연결 문자열을 1회 초기화합니다.
       MssqlServiceHub.Initialize(connectionString);

    2) 동기 읽기/쓰기 작업 (트랜잭션 경계 포함)
       // 주의: UsingServices는 트랜잭션을 자동으로 Begin/Commit/Rollback 합니다.
       //       액션(Func) 내부에서 Begin/Commit/Rollback 같은 트랜잭션 제어를 호출하지 마세요.
       var result = MssqlServiceHub.UsingServices(s => {
           // ...업무 로직 수행...
           // return 어떤 값
           return true;
       });

       // Generic 반환 타입 예시
       int count = MssqlServiceHub.UsingServices<int>(s => {
           // 예: 레이블 생성 후 저장 행 수 반환
           // 실제 구현에서는 UseCase 메서드 호출
           return 1;
       });

       // UseCase 메서드 호출 예시 (동기 래핑)
       bool created = MssqlServiceHub.UsingServices<bool>(s => {
           // 동기 컨텍스트이므로 비동기 UseCase는 결과를 동기적으로 기다립니다.
           var entity = new MstBtrEntity { UseYn = "Y" }; // 기타 필드 설정
           return s.CreateLabel.CreateAsync(entity).GetAwaiter().GetResult();
       });

       bool acknowledged = MssqlServiceHub.UsingServices<bool>(s => {
           return s.AcknowledgeCommand.AcknowledgeAsync(EIfCmd.AA3, "LABEL123").GetAwaiter().GetResult();
       });

    3) 동기 읽기 전용 작업 (트랜잭션 시작하지 않음)
       var dto = MssqlServiceHub.UsingReadOnlyServices(s => {
           return s.MetadataUseCases; // 또는 repositories/queries를 활용하는 UseCase 결과 사용
       });

       // Generic 반환 타입 예시
       string latestGrade = MssqlServiceHub.UsingReadOnlyServices<string>(s => {
           // 예: 최근 등급 문자열 반환 (실제에선 s.GradeLookup.GetLatestGradeAsync를 동기 래핑하거나 결과 캐시 사용)
           return "A";
       });

       // UseCase 메서드 호출 예시 (동기 래핑)
       string latest = MssqlServiceHub.UsingReadOnlyServices<string>(s => {
           return s.GradeLookup.GetLatestGradeAsync("LABEL123").GetAwaiter().GetResult();
       });

       IReadOnlyList<string> carMakes = MssqlServiceHub.UsingReadOnlyServices<IReadOnlyList<string>>(s => {
           return s.FilterMetadata.GetCarMakeNamesAsync().GetAwaiter().GetResult();
       });

    4) 비동기 읽기/쓰기 작업 (트랜잭션 경계 포함)
       // 주의: UsingServicesAsync도 트랜잭션을 자동으로 Begin/Commit/Rollback 합니다.
       //       액션(Func) 내부에서 트랜잭션 제어를 호출하지 마세요.
       var ok = await MssqlServiceHub.UsingServicesAsync(async s => {
           // UseCase 예: 라벨 생성 후 슬롯 할당
           var label = new MstBtrEntity { UseYn = "Y" }; // 기타 필드 지정
           var slot = new WarehouseSlotUpdateDto(); // 대상 슬롯 파라미터 지정
           return await s.CreateLabelAndAssignSlot.ExecuteAsync(label, slot);
       });

       // Generic 반환 타입 예시
       int affected = await MssqlServiceHub.UsingServicesAsync<int>(async s => {
           // 예: 저장된 행 수 반환
           await Task.Yield();
           return 1;
       });

       // UseCase 메서드 호출 예시
       bool enqueued = await MssqlServiceHub.UsingServicesAsync(async s => {
           return await s.EnqueueCommand.EnqueueAsync(EIfCmd.AA3, "LABEL123", null, null, null, "SYS");
       });

       bool transferRequested = await MssqlServiceHub.UsingServicesAsync(async s => {
           return await s.RequestTransfer.RequestAcceptAsync("LABEL123", 1, 2, 3, "SYS");
       });

    5) 비동기 읽기 전용 작업 (트랜잭션 시작하지 않음)
       var list = await MssqlServiceHub.UsingReadOnlyServicesAsync(async s => {
           return await s.FilterMetadata.GetCarMakeNamesAsync();
       });

       // Generic 반환 타입 예시
       IReadOnlyList<string> names = await MssqlServiceHub.UsingReadOnlyServicesAsync<IReadOnlyList<string>>(async s => {
           return await s.FilterMetadata.GetCarMakeNamesAsync();
       });

       // UseCase 메서드 호출 예시
       string grade = await MssqlServiceHub.UsingReadOnlyServicesAsync(async s => {
           return await s.GradeLookup.GetLatestGradeAsync("LABEL123");
       });

    비고
    - WPF UI 컨텍스트에서 동기 메서드 호출 시, 내부적으로 Task.Run으로 백그라운드에서 실행해 UI 블로킹을 회피합니다.
    - UsingServices/UsingServicesAsync는 예외 발생 시 안전하게 롤백하고 기본값(default(T))을 반환합니다.
    - Services 인스턴스는 using 블록 종료 시 자동 Dispose 되어 DbContext/UoW가 정리됩니다.
    */
    // 단일 진입점: Db/UoW + 모든 Repo/Query/UseCase를 손쉽게 사용하도록 제공
    public static class MssqlServiceHub
    {
        // =====================
        // PUBLIC METHODS
        // =====================
        /// <summary>
        /// 라이브러리에서 사용할 DB 연결 문자열을 1회 설정합니다. 중복 호출 시 InvalidOperationException을 발생시킵니다.
        /// </summary>
        /// <param name="connectionString">ADO.NET 연결 문자열</param>
        public static void Initialize(string connectionString)
        {
            if (DbConnectionSettings.IsInitialized)
                throw new InvalidOperationException("MssqlServiceHub.Initialize can only be called once per process.");

            DbConnectionSettings.Initialize(connectionString);
        }

        /// <summary>
        /// 서비스 묶음을 동기 실행합니다. 트랜잭션을 자동으로 시작/커밋하며, 예외 시 롤백 후 default(T)를 반환합니다.
        /// WPF UI 컨텍스트에서 호출되면 Task.Run으로 백그라운드에서 실행됩니다.
        /// </summary>
        /// <typeparam name="T">액션 결과 타입</typeparam>
        /// <param name="action">Services를 받아 결과를 반환하는 동기 작업</param>
        /// <returns>액션 결과 또는 예외 시 default(T)</returns>
        public static T UsingServices<T>(Func<Services, T> action)
        {
            return UsingServicesCore(action, readOnly: false);
        }

        /// <summary>
        /// 서비스 묶음을 동기 읽기 전용으로 실행합니다. 트랜잭션을 시작하지 않으며, 예외 시 default(T)를 반환합니다.
        /// </summary>
        /// <typeparam name="T">액션 결과 타입</typeparam>
        /// <param name="action">Services를 받아 결과를 반환하는 동기 작업</param>
        /// <returns>액션 결과 또는 예외 시 default(T)</returns>
        public static T UsingReadOnlyServices<T>(Func<Services, T> action)
        {
            return UsingServicesCore(action, readOnly: true);
        }

        /// <summary>
        /// 서비스 묶음을 비동기 실행합니다. 트랜잭션을 자동으로 시작/커밋하며, 예외 시 롤백 후 default(T)를 반환합니다.
        /// </summary>
        /// <typeparam name="T">액션 결과 타입</typeparam>
        /// <param name="action">Services를 받아 Task&lt;T&gt;를 반환하는 비동기 작업</param>
        /// <returns>액션 결과 또는 예외 시 default(T)</returns>
        public static Task<T> UsingServicesAsync<T>(Func<Services, Task<T>> action)
        {
            return UsingServicesAsyncCore(action, readOnly: false);
        }

        /// <summary>
        /// 서비스 묶음을 비동기 읽기 전용으로 실행합니다. 트랜잭션을 시작하지 않으며, 예외 시 default(T)를 반환합니다.
        /// </summary>
        /// <typeparam name="T">액션 결과 타입</typeparam>
        /// <param name="action">Services를 받아 Task&lt;T&gt;를 반환하는 비동기 작업</param>
        /// <returns>액션 결과 또는 예외 시 default(T)</returns>
        public static Task<T> UsingReadOnlyServicesAsync<T>(Func<Services, Task<T>> action)
        {
            return UsingServicesAsyncCore(action, readOnly: true);
        }

        // =====================
        // NESTED TYPES
        // =====================
        /// <summary>
        /// 자주 쓰는 Repositories/Queries와 UseCase들을 한 번에 제공하는 컨테이너.
        /// 트랜잭션 경계 제어(Begin/Commit/Rollback) API를 위임합니다.
        /// </summary>
        public sealed class Services : IDisposable
        {
            // 내부 관리용 컨텍스트/단위작업
            private readonly IAppDbContext _db;
            private readonly IUnitOfWork _uow;
            private bool _disposed;

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

            /// <summary>
            /// 트랜잭션을 시작합니다. 일반적으로 UsingServices/UsingServicesAsync에서 자동으로 호출되므로 직접 사용할 필요는 없습니다.
            /// </summary>
            public void Begin() { _uow.Begin(); }
            /// <summary>
            /// 트랜잭션을 커밋합니다.
            /// </summary>
            public void Commit() { _uow.Commit(); }
            /// <summary>
            /// 트랜잭션을 롤백합니다.
            /// </summary>
            public void Rollback() { _uow.Rollback(); }
            /// <summary>
            /// 트랜잭션을 비동기로 시작합니다.
            /// </summary>
            public Task BeginAsync() { return _uow.BeginAsync(); }
            /// <summary>
            /// 트랜잭션을 비동기로 커밋합니다.
            /// </summary>
            public Task CommitAsync() { return _uow.CommitAsync(); }
            /// <summary>
            /// 트랜잭션을 비동기로 롤백합니다.
            /// </summary>
            public Task RollbackAsync() { return _uow.RollbackAsync(); }

            /// <summary>
            /// 내부 리소스를 해제합니다. UoW를 우선적으로 Dispose하여 활성 트랜잭션을 안전하게 종료합니다.
            /// </summary>
            public void Dispose()
            {
                if (_disposed) return;
                try
                {
                    // Ensure UoW is disposed first to gracefully end any active transaction it owns
                    try { _uow?.Dispose(); } catch { /* swallow */ }
                    try { _db?.Dispose(); } catch { /* swallow */ }
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        // =====================
        // PRIVATE METHODS
        // =====================
        /// <summary>
        /// Services 인스턴스를 생성합니다. 내부적으로 DbContext와 UnitOfWork를 초기화합니다.
        /// </summary>
        private static Services CreateServices()
        {
            var factory = new AppDbContextFactory(DbConnectionSettings.ConnectionString);
            var db = factory.Create();
            var uow = new GbtpLib.Mssql.Persistence.UnitOfWork.EfUnitOfWork(db);
            return new Services(db, uow);
        }

        /// <summary>
        /// 현재 SynchronizationContext가 WPF Dispatcher 컨텍스트인지 판별합니다.
        /// </summary>
        private static bool IsUiContext(SynchronizationContext ctx)
        {
            // WPF 전용: WindowsBase의 DispatcherSynchronizationContext 타입을 리플렉션으로 확인
            // (라이브러리가 WPF 어셈블리에 직접 의존하지 않도록 함)
            if (ctx == null) return false;
            var dispCtxType = Type.GetType("System.Windows.Threading.DispatcherSynchronizationContext, WindowsBase", false);
            return dispCtxType != null && dispCtxType.IsInstanceOfType(ctx);
        }

        /// <summary>
        /// 동기 실행 코어 로직. readOnly 여부에 따라 트랜잭션을 시작/커밋하거나 생략합니다. 예외 시 롤백 후 default(T)를 반환합니다.
        /// </summary>
        private static T UsingServicesCore<T>(Func<Services, T> action, bool readOnly)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Func<T> execute = () =>
            {
                using (var services = CreateServices())
                {
                    if (readOnly)
                    {
                        try
                        {
                            return action(services);
                        }
                        catch
                        {
                            return default(T);
                        }
                    }

                    try
                    {
                        services.Begin();
                        var result = action(services);
                        services.Commit();
                        return result;
                    }
                    catch
                    {
                        try { services.Rollback(); } catch { /* swallow rollback errors */ }
                        return default(T);
                    }
                }
            };

            var ctx = SynchronizationContext.Current;
            if (IsUiContext(ctx))
            {
                return Task.Run(execute).GetAwaiter().GetResult();
            }

            return execute();
        }

        /// <summary>
        /// 비동기 실행 코어 로직. readOnly 여부에 따라 트랜잭션을 시작/커밋하거나 생략합니다. 예외 시 롤백 후 default(T)를 반환합니다.
        /// </summary>
        private static async Task<T> UsingServicesAsyncCore<T>(Func<Services, Task<T>> action, bool readOnly)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            using (var services = CreateServices())
            {
                if (readOnly)
                {
                    try
                    {
                        var result = await action(services).ConfigureAwait(false);
                        return result;
                    }
                    catch
                    {
                        // read-only에서 예외가 발생하면 기본값 반환
                        return default(T);
                    }
                }

                try
                {
                    await services.BeginAsync().ConfigureAwait(false);
                    var result = await action(services).ConfigureAwait(false);
                    await services.CommitAsync().ConfigureAwait(false);
                    return result;
                }
                catch
                {
                    try { await services.RollbackAsync().ConfigureAwait(false); } catch { /* swallow rollback errors */ }
                    return default(T);
                }
            }
        }
    }
}
