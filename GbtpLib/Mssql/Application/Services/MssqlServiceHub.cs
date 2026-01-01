using System;
using System.Collections.Generic;
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
using GbtpLib.Logging; // add logging

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
           return s.Labels.CreateLabelAsync(entity).GetAwaiter().GetResult();
       });

       bool acknowledged = MssqlServiceHub.UsingServices<bool>(s => {
           return s.InterfaceCommands.AcknowledgeAsync(EIfCmd.AA3, "LABEL123").GetAwaiter().GetResult();
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
           return await s.Labels.CreateLabelAndAssignSlotAsync(label, slot);
       });

       // Generic 반환 타입 예시
       int affected = await MssqlServiceHub.UsingServicesAsync<int>(async s => {
           // 예: 저장된 행 수 반환
           await Task.Yield();
           return 1;
       });

       // UseCase 메서드 호출 예시
       bool enqueued = await MssqlServiceHub.UsingServicesAsync(async s => {
           return await s.InterfaceCommands.EnqueueAsync(EIfCmd.AA3, "LABEL123", null, null, null, "SYS");
       });

       bool transferRequested = await MssqlServiceHub.UsingServicesAsync(async s => {
           return await s.InterfaceCommands.RequestAcceptAsync("LABEL123", 1, 2, 3, "SYS");
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
        /// 라이브러리에서 사용할 DB 연결 문자열을 1회 설정합니다. 중복 호출 시 InvalidOperationException을 발생시킺니다.
        /// </summary>
        /// <param name="connectionString">ADO.NET 연결 문자열</param>
        public static void Initialize(string connectionString)
        {
            try
            {
                if (DbConnectionSettings.IsInitialized)
                    throw new InvalidOperationException("MssqlServiceHub.Initialize can only be called once per process.");

                DbConnectionSettings.Initialize(connectionString);
            }
            catch (Exception ex)
            {
                AppLog.Error("MssqlServiceHub.Initialize failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// 서비스 묶음을 동기 실행합니다. 트랜잭션을 자동으로 시작/커밋하며, 예외 시 롤백 후 default(T)을 반환합니다.
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

            // 내부 관리용 Repositories / Queries (Lazy 초기화)
            private readonly Lazy<IMstUserInfoRepository> _users;
            private readonly Lazy<IInvWarehouseRepository> _warehouses;
            private readonly Lazy<IItfCmdDataRepository> _cmdRepo;
            private readonly Lazy<IItfCmdDataQueries> _cmdQueries;
            private readonly Lazy<IStoredProcedureExecutor> _storedProc;
            private readonly Lazy<IMstCodeRepository> _codes;
            private readonly Lazy<ISlotQueryRepository> _slots;
            private readonly Lazy<IMstBtrRepository> _batteries;
            private readonly Lazy<IMstBtrTypeRepository> _batteryTypes;
            private readonly Lazy<IMetadataQueries> _metadata;
            private readonly Lazy<ILabelCreationQueries> _labelCreation;
            private readonly Lazy<IQltBtrInspQueries> _inspection;
            private readonly Lazy<IDefectBatteryQueries> _defects;
            private readonly Lazy<IWarehouseQueries> _warehouseQueries;
            private readonly Lazy<IQltBtrInoutInspRepository> _inoutInspectionRepo;

            // 공개 UseCases (Lazy 초기화)
            private readonly Lazy<LoginUseCases> _login;
            private readonly Lazy<GetCodeUseCases> _getCode;
            private readonly Lazy<MetadataUseCases> _metadataUseCases;
            private readonly Lazy<WarehouseSlotUseCases> _warehouseSlotUseCases;
            private readonly Lazy<WarehouseSlotSearchUseCases> _warehouseSlotSearchUseCases;
            private readonly Lazy<GradeLookupUseCases> _gradeLookup;
            private readonly Lazy<FilterMetadataUseCases> _filterMetadata;
            private readonly Lazy<InterfaceCommandUseCases> _interfaceCommandUseCases;
            private readonly Lazy<LabelManagementUseCases> _labelManagementUseCases;
            private readonly Lazy<InoutInspectionUseCases> _inoutInspectionUseCases;
            private readonly Lazy<InoutInspectionSaveUseCases> _inoutInspectionSaveUseCases;
            private readonly Lazy<StoredProcCommandUseCases> _storedProcCommandUseCases;

            // 공개 접근자 (Lazy.Value 반환)
            public LoginUseCases Login { get { return _login.Value; } }
            public GetCodeUseCases GetCode { get { return _getCode.Value; } }
            public MetadataUseCases MetadataUseCases { get { return _metadataUseCases.Value; } }
            public WarehouseSlotUseCases Slots { get { return _warehouseSlotUseCases.Value; } }
            public WarehouseSlotSearchUseCases SlotSearch { get { return _warehouseSlotSearchUseCases.Value; } }
            public GradeLookupUseCases GradeLookup { get { return _gradeLookup.Value; } }
            public FilterMetadataUseCases FilterMetadata { get { return _filterMetadata.Value; } }
            public InterfaceCommandUseCases InterfaceCommands { get { return _interfaceCommandUseCases.Value; } }
            public LabelManagementUseCases Labels { get { return _labelManagementUseCases.Value; } }
            public InoutInspectionUseCases InoutInspection { get { return _inoutInspectionUseCases.Value; } }
            public InoutInspectionSaveUseCases InoutInspectionSave { get { return _inoutInspectionSaveUseCases.Value; } }
            public StoredProcCommandUseCases StoredProcCommands { get { return _storedProcCommandUseCases.Value; } }

            internal Services(IAppDbContext db, IUnitOfWork uow)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                _uow = uow ?? throw new ArgumentNullException(nameof(uow));

                try
                {
                    // repos/queries (지연 생성)
                    _users = new Lazy<IMstUserInfoRepository>(() => new MstUserInfoRepository(_db), LazyThreadSafetyMode.None);
                    _warehouses = new Lazy<IInvWarehouseRepository>(() => new InvWarehouseRepository(_db), LazyThreadSafetyMode.None);
                    _cmdRepo = new Lazy<IItfCmdDataRepository>(() => new ItfCmdDataRepository(_db), LazyThreadSafetyMode.None);
                    _cmdQueries = new Lazy<IItfCmdDataQueries>(() => new ItfCmdDataQueries(_db), LazyThreadSafetyMode.None);
                    _storedProc = new Lazy<IStoredProcedureExecutor>(() => new StoredProcedureExecutor(_db), LazyThreadSafetyMode.None);
                    _codes = new Lazy<IMstCodeRepository>(() => new MstCodeRepository(_db), LazyThreadSafetyMode.None);
                    _slots = new Lazy<ISlotQueryRepository>(() => new SlotQueryRepository(_db), LazyThreadSafetyMode.None);
                    _batteries = new Lazy<IMstBtrRepository>(() => new MstBtrRepository(_db), LazyThreadSafetyMode.None);
                    _batteryTypes = new Lazy<IMstBtrTypeRepository>(() => new MstBtrTypeRepository(_db), LazyThreadSafetyMode.None);
                    _metadata = new Lazy<IMetadataQueries>(() => new MetadataQueries(_db), LazyThreadSafetyMode.None);
                    _labelCreation = new Lazy<ILabelCreationQueries>(() => new LabelCreationQueries(_db), LazyThreadSafetyMode.None);
                    _inspection = new Lazy<IQltBtrInspQueries>(() => new QltBtrInspQueries(_db), LazyThreadSafetyMode.None);
                    _defects = new Lazy<IDefectBatteryQueries>(() => new DefectBatteryQueries(_db), LazyThreadSafetyMode.None);
                    _warehouseQueries = new Lazy<IWarehouseQueries>(() => new WarehouseQueries(_db), LazyThreadSafetyMode.None);
                    _inoutInspectionRepo = new Lazy<IQltBtrInoutInspRepository>(() => new QltBtrInoutInspRepository(_db), LazyThreadSafetyMode.None);

                    // usecases (지연 생성)
                    _login = new Lazy<LoginUseCases>(() => new LoginUseCases(_users.Value), LazyThreadSafetyMode.None);
                    _getCode = new Lazy<GetCodeUseCases>(() => new GetCodeUseCases(_codes.Value), LazyThreadSafetyMode.None);
                    _metadataUseCases = new Lazy<MetadataUseCases>(() => new MetadataUseCases(_metadata.Value), LazyThreadSafetyMode.None);
                    _warehouseSlotUseCases = new Lazy<WarehouseSlotUseCases>(() => new WarehouseSlotUseCases(_slots.Value, _warehouses.Value), LazyThreadSafetyMode.None);
                    _warehouseSlotSearchUseCases = new Lazy<WarehouseSlotSearchUseCases>(() => new WarehouseSlotSearchUseCases(_slots.Value), LazyThreadSafetyMode.None);
                    _gradeLookup = new Lazy<GradeLookupUseCases>(() => new GradeLookupUseCases(_inspection.Value), LazyThreadSafetyMode.None);
                    _filterMetadata = new Lazy<FilterMetadataUseCases>(() => new FilterMetadataUseCases(_metadata.Value), LazyThreadSafetyMode.None);
                    _interfaceCommandUseCases = new Lazy<InterfaceCommandUseCases>(() => new InterfaceCommandUseCases(_cmdRepo.Value, _cmdQueries.Value, _storedProc.Value), LazyThreadSafetyMode.None);
                    _labelManagementUseCases = new Lazy<LabelManagementUseCases>(() => new LabelManagementUseCases(_batteries.Value, _warehouses.Value, _labelCreation.Value, _batteryTypes.Value), LazyThreadSafetyMode.None);
                    _inoutInspectionUseCases = new Lazy<InoutInspectionUseCases>(() => new InoutInspectionUseCases(_inspection.Value), LazyThreadSafetyMode.None);
                    _inoutInspectionSaveUseCases = new Lazy<InoutInspectionSaveUseCases>(() => new InoutInspectionSaveUseCases(_inoutInspectionRepo.Value), LazyThreadSafetyMode.None);
                    _storedProcCommandUseCases = new Lazy<StoredProcCommandUseCases>(() => new StoredProcCommandUseCases(_storedProc.Value), LazyThreadSafetyMode.None);
                }
                catch (Exception ex)
                {
                    AppLog.Error("MssqlServiceHub.Services initialization failed.", ex);
                    throw;
                }
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
            try
            {
                var factory = new AppDbContextFactory(DbConnectionSettings.ConnectionString);
                var db = factory.Create();
                var uow = new GbtpLib.Mssql.Persistence.UnitOfWork.EfUnitOfWork(db);
                return new Services(db, uow);
            }
            catch (Exception ex)
            {
                AppLog.Error("MssqlServiceHub.CreateServices failed.", ex);
                throw;
            }
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
                try
                {
                    using (var services = CreateServices())
                    {
                        if (readOnly)
                        {
                            try
                            {
                                return action(services);
                            }
                            catch (Exception ex)
                            {
                                try { AppLog.Error("UsingReadOnlyServices failed.", ex); } catch { }
                                // Lazy.Value 등 초기화 실패 시 새 Services로 1회 재시도
                                try
                                {
                                    using (var retryServices = CreateServices())
                                    {
                                        return action(retryServices);
                                    }
                                }
                                catch (Exception rex)
                                {
                                    try { AppLog.Error("UsingReadOnlyServices retry failed.", rex); } catch { }
                                    return default(T);
                                }
                            }
                        }

                        try
                        {
                            services.Begin();
                            var result = action(services);
                            services.Commit();
                            return result;
                        }
                        catch (Exception ex)
                        {
                            try { services.Rollback(); } catch { /* swallow rollback errors */ }
                            try { AppLog.Error("UsingServices failed.", ex); } catch { }
                            // Lazy.Value 등 초기화 실패 시 새 Services로 1회 재시도 (새 트랜잭션)
                            try
                            {
                                using (var retryServices = CreateServices())
                                {
                                    retryServices.Begin();
                                    var retryResult = action(retryServices);
                                    retryServices.Commit();
                                    return retryResult;
                                }
                            }
                            catch (Exception rex)
                            {
                                // 재시도 실패 시 롤백 및 기본값 반환
                                try { AppLog.Error("UsingServices retry failed.", rex); } catch { }
                                return default(T);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { AppLog.Error("UsingServicesCore service creation failed.", ex); } catch { }
                    return default(T);
                }
            };

            var ctx = SynchronizationContext.Current;
            if (IsUiContext(ctx))
            {
                try
                {
                    return Task.Run(execute).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    try { AppLog.Error("UsingServicesCore Task.Run failed.", ex); } catch { }
                    return default(T);
                }
            }

            return execute();
        }

        /// <summary>
        /// 비동기 실행 코어 로직. readOnly 여부에 따라 트랜잭션을 시작/커밋하거나 생략합니다. 예외 시 롤백 후 default(T>)을 반환합니다.
        /// </summary>
        private static async Task<T> UsingServicesAsyncCore<T>(Func<Services, Task<T>> action, bool readOnly)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            try
            {
                using (var services = CreateServices())
                {
                    if (readOnly)
                    {
                        try
                        {
                            var result = await action(services).ConfigureAwait(false);
                            return result;
                        }
                        catch (Exception ex)
                        {
                            try { AppLog.Error("UsingReadOnlyServicesAsync failed.", ex); } catch { }
                            // Lazy.Value 등 초기화 실패 시 새 Services로 1회 재시도
                            try
                            {
                                using (var retryServices = CreateServices())
                                {
                                    var retryResult = await action(retryServices).ConfigureAwait(false);
                                    return retryResult;
                                }
                            }
                            catch (Exception rex)
                            {
                                try { AppLog.Error("UsingReadOnlyServicesAsync retry failed.", rex); } catch { }
                                return default(T);
                            }
                        }
                    }

                    try
                    {
                        await services.BeginAsync().ConfigureAwait(false);
                        var result = await action(services).ConfigureAwait(false);
                        await services.CommitAsync().ConfigureAwait(false);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        try { await services.RollbackAsync().ConfigureAwait(false); } catch { /* swallow rollback errors */ }
                        try { AppLog.Error("UsingServicesAsync failed.", ex); } catch { }
                        // Lazy.Value 등 초기화 실패 시 새 Services로 1회 재시도 (새 트랜잭션)
                        try
                        {
                            using (var retryServices = CreateServices())
                            {
                                await retryServices.BeginAsync().ConfigureAwait(false);
                                var retryResult = await action(retryServices).ConfigureAwait(false);
                                await retryServices.CommitAsync().ConfigureAwait(false);
                                return retryResult;
                            }
                        }
                        catch (Exception rex)
                        {
                            try { AppLog.Error("UsingServicesAsync retry failed.", rex); } catch { }
                            return default(T);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try { AppLog.Error("UsingServicesAsyncCore service creation failed.", ex); } catch { }
                return default(T);
            }
        }
    }
}
