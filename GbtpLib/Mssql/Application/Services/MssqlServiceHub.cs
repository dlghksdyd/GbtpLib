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
using GbtpLib.Logging; // add logging

namespace GbtpLib.Mssql.Application.Services
{
    public static class MssqlServiceHub
    {
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

        public static T UsingServices<T>(Func<Services, T> action)
        {
            return UsingServicesCore(action, readOnly: false);
        }

        public static T UsingReadOnlyServices<T>(Func<Services, T> action)
        {
            return UsingServicesCore(action, readOnly: true);
        }

        public static Task<T> UsingServicesAsync<T>(Func<Services, Task<T>> action)
        {
            return UsingServicesAsyncCore(action, readOnly: false);
        }

        public static Task<T> UsingReadOnlyServicesAsync<T>(Func<Services, Task<T>> action)
        {
            return UsingServicesAsyncCore(action, readOnly: true);
        }

        public sealed class Services : IDisposable
        {
            private readonly IAppDbContext _db;
            private readonly IUnitOfWork _uow;
            private bool _disposed;

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
            private readonly Lazy<ILabelInfoLookupUseCase> _labelInfoLookup;

            private readonly Lazy<LoginUseCase> _login;
            private readonly Lazy<GetCodeUseCase> _getCode;
            private readonly Lazy<MetadataUseCases> _metadataUseCases;
            private readonly Lazy<WarehouseSlotUseCases> _warehouseSlotUseCases;
            private readonly Lazy<OutcomeFlowUseCases> _outcomeFlow;
            private readonly Lazy<FilterMetadataUseCase> _filterMetadata;
            private readonly Lazy<InterfaceCommandUseCases> _interfaceCommandUseCases;
            private readonly Lazy<LabelManagementUseCases> _labelManagementUseCases;
            private readonly Lazy<QltBtrInoutInspectionUseCases> _inoutInspectionUseCases;
            private readonly Lazy<DefectBatteryUseCases> _defectUseCases;

            public LoginUseCase Login { get { return _login.Value; } }
            public GetCodeUseCase GetCode { get { return _getCode.Value; } }
            public MetadataUseCases MetadataUseCases { get { return _metadataUseCases.Value; } }
            public WarehouseSlotUseCases Slots { get { return _warehouseSlotUseCases.Value; } }
            public OutcomeFlowUseCases OutcomeFlow { get { return _outcomeFlow.Value; } }
            public FilterMetadataUseCase FilterMetadata { get { return _filterMetadata.Value; } }
            public InterfaceCommandUseCases InterfaceCommands { get { return _interfaceCommandUseCases.Value; } }
            public LabelManagementUseCases Labels { get { return _labelManagementUseCases.Value; } }
            public DefectBatteryUseCases Defects { get { return _defectUseCases.Value; } }
            public QltBtrInoutInspectionUseCases InoutInspections { get { return _inoutInspectionUseCases.Value; } }
            public ILabelInfoLookupUseCase LabelInfoLookup { get { return _labelInfoLookup.Value; } }

            internal Services(IAppDbContext db, IUnitOfWork uow)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                _uow = uow ?? throw new ArgumentNullException(nameof(uow));

                try
                {
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
                    _labelInfoLookup = new Lazy<ILabelInfoLookupUseCase>(() => new LabelInfoLookupUseCase(_db), LazyThreadSafetyMode.None);

                    _login = new Lazy<LoginUseCase>(() => new LoginUseCase(_users.Value), LazyThreadSafetyMode.None);
                    _getCode = new Lazy<GetCodeUseCase>(() => new GetCodeUseCase(_codes.Value), LazyThreadSafetyMode.None);
                    _metadataUseCases = new Lazy<MetadataUseCases>(() => new MetadataUseCases(_metadata.Value), LazyThreadSafetyMode.None);
                    _warehouseSlotUseCases = new Lazy<WarehouseSlotUseCases>(() => new WarehouseSlotUseCases(_slots.Value, _warehouses.Value), LazyThreadSafetyMode.None);
                    _outcomeFlow = new Lazy<OutcomeFlowUseCases>(() => new OutcomeFlowUseCases(_slots.Value), LazyThreadSafetyMode.None);
                    _filterMetadata = new Lazy<FilterMetadataUseCase>(() => new FilterMetadataUseCase(_metadata.Value), LazyThreadSafetyMode.None);
                    _interfaceCommandUseCases = new Lazy<InterfaceCommandUseCases>(() => new InterfaceCommandUseCases(_cmdRepo.Value, _cmdQueries.Value, _storedProc.Value), LazyThreadSafetyMode.None);
                    _labelManagementUseCases = new Lazy<LabelManagementUseCases>(() => new LabelManagementUseCases(_batteries.Value, _warehouses.Value, _labelCreation.Value, _batteryTypes.Value), LazyThreadSafetyMode.None);
                    _inoutInspectionUseCases = new Lazy<QltBtrInoutInspectionUseCases>(() => new QltBtrInoutInspectionUseCases(_db), LazyThreadSafetyMode.None);
                    _defectUseCases = new Lazy<DefectBatteryUseCases>(() => new DefectBatteryUseCases(_defects.Value), LazyThreadSafetyMode.None);
                }
                catch (Exception ex)
                {
                    AppLog.Error("MssqlServiceHub.Services initialization failed." , ex);
                    throw;
                }
            }

            public void Begin() { _uow.Begin(); }
            public void Commit() { _uow.Commit(); }
            public void Rollback() { _uow.Rollback(); }
            public Task BeginAsync() { return _uow.BeginAsync(); }
            public Task CommitAsync() { return _uow.CommitAsync(); }
            public Task RollbackAsync() { return _uow.RollbackAsync(); }

            public void Dispose()
            {
                if (_disposed) return;
                try
                {
                    try { _uow?.Dispose(); } catch { }
                    try { _db?.Dispose(); } catch { }
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

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

        private static bool IsUiContext(SynchronizationContext ctx)
        {
            if (ctx == null) return false;
            var dispCtxType = Type.GetType("System.Windows.Threading.DispatcherSynchronizationContext, WindowsBase", false);
            return dispCtxType != null && dispCtxType.IsInstanceOfType(ctx);
        }

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
                            try { services.Rollback(); } catch { }
                            try { AppLog.Error("UsingServices failed.", ex); } catch { }
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
                        try { await services.RollbackAsync().ConfigureAwait(false); } catch { }
                        try { AppLog.Error("UsingServicesAsync failed.", ex); } catch { }
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
