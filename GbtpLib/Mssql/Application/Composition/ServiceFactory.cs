using System;
using GbtpLib.Configuration;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Application.UseCases;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Db;
using GbtpLib.Mssql.Persistence.Repositories;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.Composition
{
    // Lightweight composition root to obtain repositories/use-cases without external DI
    public static class ServiceFactory
    {
        public static (IAppDbContext Db, IUnitOfWork Uow) CreateDb()
        {
            var factory = new AppDbContextFactory(DbConnectionSettings.ConnectionString);
            var db = factory.Create();
            var uow = new GbtpLib.Mssql.Persistence.UnitOfWork.EfUnitOfWork(db);
            return (db, uow);
        }

        public static IMstUserInfoRepository CreateUserRepo(IAppDbContext db) => new MstUserInfoRepository(db);
        public static IInvWarehouseRepository CreateWarehouseRepo(IAppDbContext db) => new InvWarehouseRepository(db);
        public static IItfCmdDataRepository CreateCmdRepo(IAppDbContext db) => new ItfCmdDataRepository(db);
        public static IItfCmdDataQueries CreateCmdQueries(IAppDbContext db) => new ItfCmdDataQueries(db);
        public static IStoredProcedureExecutor CreateSp(IAppDbContext db) => new StoredProcedureExecutor(db);
        public static IMstCodeRepository CreateCodeRepo(IAppDbContext db) => new MstCodeRepository(db);
        public static ISlotQueryRepository CreateSlotQueries(IAppDbContext db) => new SlotQueryRepository(db);
        public static IMstBtrRepository CreateBtrRepo(IAppDbContext db) => new MstBtrRepository(db);
        public static IMstBtrTypeRepository CreateBtrTypeRepo(IAppDbContext db) => new MstBtrTypeRepository(db);
        public static IMetadataQueries CreateMetadataQueries(IAppDbContext db) => new MetadataQueries(db);
        public static ILabelCreationQueries CreateLabelCreationQueries(IAppDbContext db) => new LabelCreationQueries(db);
        public static IQltBtrInspQueries CreateInspQueries(IAppDbContext db) => new QltBtrInspQueries(db);
        public static IDefectBatteryQueries CreateDefectBatteryQueries(IAppDbContext db) => new DefectBatteryQueries(db);
        public static IWarehouseQueries CreateWarehouseQueries(IAppDbContext db) => new WarehouseQueries(db);
        public static IWarehouseLayoutQueries CreateWarehouseLayoutQueries(IAppDbContext db) => new WarehouseLayoutQueries(db);

        public static LoginUseCase CreateLoginUseCase(IUnitOfWork uow, IMstUserInfoRepository userRepo) => new LoginUseCase(uow, userRepo);
        public static GetCodeUseCase CreateGetCodeUseCase(IUnitOfWork uow, IMstCodeRepository codeRepo) => new GetCodeUseCase(uow, codeRepo);
        public static MetadataUseCases CreateMetadataUseCases(IUnitOfWork uow, IMetadataQueries queries) => new MetadataUseCases(uow, queries);
        public static InitializeSlotsUseCase CreateInitializeSlotsUseCase(IUnitOfWork uow, ISlotQueryRepository repo) => new InitializeSlotsUseCase(uow, repo);
        public static UpdateWarehouseSlotUseCase CreateUpdateWarehouseSlotUseCase(IUnitOfWork uow, IInvWarehouseRepository repo) => new UpdateWarehouseSlotUseCase(uow, repo);
        public static EnqueueCommandUseCase CreateEnqueueCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository repo) => new EnqueueCommandUseCase(uow, repo);
        public static AcknowledgeCommandUseCase CreateAcknowledgeCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository repo) => new AcknowledgeCommandUseCase(uow, repo);
        public static RequestTransferUseCase CreateRequestTransferUseCase(IUnitOfWork uow, IStoredProcedureExecutor sp) => new RequestTransferUseCase(uow, sp);
        public static CommandPollingUseCase CreateCommandPollingUseCase(IUnitOfWork uow, IItfCmdDataQueries q, IItfCmdDataRepository r) => new CommandPollingUseCase(uow, q, r);
        public static CreateLabelUseCase CreateCreateLabelUseCase(IUnitOfWork uow, IMstBtrRepository r) => new CreateLabelUseCase(uow, r);
        public static DeleteLabelFlowUseCase CreateDeleteLabelFlowUseCase(IUnitOfWork uow, IMstBtrRepository b, IInvWarehouseRepository w) => new DeleteLabelFlowUseCase(uow, b, w);
        public static LabelCreationMetadataUseCase CreateLabelCreationMetadataUseCase(IUnitOfWork uow, ILabelCreationQueries q) => new LabelCreationMetadataUseCase(uow, q);
        public static LabelCreationUseCase CreateLabelCreationUseCase(IUnitOfWork uow, IMstBtrTypeRepository t, IMstBtrRepository b) => new LabelCreationUseCase(uow, t, b);
        public static GradeLookupUseCase CreateGradeLookupUseCase(IUnitOfWork uow, IQltBtrInspQueries q) => new GradeLookupUseCase(uow, q);
        public static CreateLabelAndAssignSlotUseCase CreateCreateLabelAndAssignSlotUseCase(IUnitOfWork uow, IMstBtrRepository b, IInvWarehouseRepository w) => new CreateLabelAndAssignSlotUseCase(uow, b, w);
        public static OutcomeFlowUseCases CreateOutcomeFlowUseCases(IUnitOfWork uow, IInvWarehouseRepository w, IStoredProcedureExecutor sp, IItfCmdDataRepository c, IItfCmdDataQueries q) => new OutcomeFlowUseCases(uow, w, sp, c, q);
        public static IncomeFlowUseCases CreateIncomeFlowUseCases(IUnitOfWork uow, IStoredProcedureExecutor sp, IItfCmdDataRepository c, IItfCmdDataQueries q, IInvWarehouseRepository w) => new IncomeFlowUseCases(uow, sp, c, q, w);
        public static FilterMetadataUseCase CreateFilterMetadataUseCase(IUnitOfWork uow, IMetadataQueries q) => new FilterMetadataUseCase(uow, q);
    }
}
