using System.Data.Entity;
using GbtpLib.Mssql.Persistence.Configurations;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Db
{
    // Actual EF6 DbContext for our schema
    public class AppEfDbContext : DbContext
    {
        public AppEfDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            // Disable initializer by default; consumer can set their own.
            Database.SetInitializer<AppEfDbContext>(null);
        }

        public DbSet<InvWarehouseEntity> InvWarehouses { get; set; }
        public DbSet<InvWarehouseHistEntity> InvWarehouseHists { get; set; }
        public DbSet<ItfCmdDataEntity> ItfCmdData { get; set; }
        public DbSet<ItfCmdDataHistEntity> ItfCmdDataHists { get; set; }
        public DbSet<ItfDigCmdDataEntity> ItfDigCmdData { get; set; }
        public DbSet<ItfDigCmdDataHistEntity> ItfDigCmdDataHists { get; set; }
        public DbSet<ItfInoutCmdDataEntity> ItfInoutCmdData { get; set; }
        public DbSet<ItfInoutCmdDataHistEntity> ItfInoutCmdDataHists { get; set; }
        public DbSet<ItfSimEntity> ItfSims { get; set; }
        public DbSet<ItfSimMstEntity> ItfSimMsts { get; set; }
        public DbSet<ItfSysConCheckEntity> ItfSysConChecks { get; set; }
        public DbSet<ItfWmsCmdDataEntity> ItfWmsCmdData { get; set; }
        public DbSet<ItfWmsCmdDataHistEntity> ItfWmsCmdDataHists { get; set; }

        public DbSet<MstAgvWrkRankEntity> AgvWorkRanks { get; set; }
        public DbSet<MstBtrTypeEntity> BatteryTypes { get; set; }
        public DbSet<MstBtrMakeEntity> BatteryMakes { get; set; }
        public DbSet<MstCarMakeEntity> CarMakes { get; set; }
        public DbSet<MstCarEntity> Cars { get; set; }
        public DbSet<MstCodeEntity> Codes { get; set; }
        public DbSet<MstCodeGroupEntity> CodeGroups { get; set; }
        public DbSet<MstCustomerEntity> Customers { get; set; }
        public DbSet<MstDeptEntity> Depts { get; set; }
        public DbSet<MstDigItemEntity> DigItems { get; set; }
        public DbSet<MstFactoryEntity> Factories { get; set; }
        public DbSet<MstInspKindEntity> InspKinds { get; set; }
        public DbSet<MstInspKindGroupEntity> InspKindGroups { get; set; }
        public DbSet<MstMachineEntity> Machines { get; set; }
        public DbSet<MstMachineChannelEntity> MachineChannels { get; set; }
        public DbSet<MstMachineStsEntity> MachineStatuses { get; set; }
        public DbSet<MstPopEntity> Pops { get; set; }
        public DbSet<MstPopMachineEntity> PopMachines { get; set; }
        public DbSet<MstProcessEntity> Processes { get; set; }

        public DbSet<MstWarehouseEntity> Warehouses { get; set; }
        public DbSet<MstWarehouseCellEntity> WarehouseCells { get; set; }
        public DbSet<SysMenuMstEntity> Menus { get; set; }
        public DbSet<SysGridEntity> Grids { get; set; }

        public DbSet<QltBtrInspEntity> QltBtrInsps { get; set; }
        public DbSet<QltBtrInoutInspEntity> QltBtrInoutInsps { get; set; }
        public DbSet<QltBtrDigInspDtlResultEntity> QltBtrDigInspDtlResults { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new InvWarehouseConfiguration());
            modelBuilder.Configurations.Add(new InvWarehouseHistConfiguration());
            modelBuilder.Configurations.Add(new ItfCmdDataConfiguration());
            modelBuilder.Configurations.Add(new ItfCmdDataHistConfiguration());
            modelBuilder.Configurations.Add(new ItfDigCmdDataConfiguration());
            modelBuilder.Configurations.Add(new ItfDigCmdDataHistConfiguration());
            modelBuilder.Configurations.Add(new ItfInoutCmdDataConfiguration());
            modelBuilder.Configurations.Add(new ItfInoutCmdDataHistConfiguration());
            modelBuilder.Configurations.Add(new ItfSimConfiguration());
            modelBuilder.Configurations.Add(new ItfSimMstConfiguration());
            modelBuilder.Configurations.Add(new ItfSysConCheckConfiguration());
            modelBuilder.Configurations.Add(new ItfWmsCmdDataConfiguration());
            modelBuilder.Configurations.Add(new ItfWmsCmdDataHistConfiguration());
            modelBuilder.Configurations.Add(new MstAgvWrkRankConfiguration());
            modelBuilder.Configurations.Add(new MstBtrMakeConfiguration());
            modelBuilder.Configurations.Add(new MstBtrTypeConfiguration());
            modelBuilder.Configurations.Add(new MstCarConfiguration());
            modelBuilder.Configurations.Add(new MstCarMakeConfiguration());
            modelBuilder.Configurations.Add(new MstCodeConfiguration());
            modelBuilder.Configurations.Add(new MstCodeGroupConfiguration());
            modelBuilder.Configurations.Add(new MstCustomerConfiguration());
            modelBuilder.Configurations.Add(new MstDeptConfiguration());
            modelBuilder.Configurations.Add(new MstDigItemConfiguration());
            modelBuilder.Configurations.Add(new MstFactoryConfiguration());
            modelBuilder.Configurations.Add(new MstInspKindConfiguration());

            // Relationships
            modelBuilder.Entity<MstCarEntity>()
                .HasRequired<MstCarMakeEntity>(c => null)
                .WithMany()
                .HasForeignKey(c => c.CarMakeCode)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MstBtrTypeEntity>()
                .HasOptional<MstCarMakeEntity>(t => null)
                .WithMany()
                .HasForeignKey(t => t.CarMakeCode)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<MstBtrTypeEntity>()
                .HasOptional<MstCarEntity>(t => null)
                .WithMany()
                .HasForeignKey(t => t.CarCode)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<MstBtrTypeEntity>()
                .HasOptional<MstBtrMakeEntity>(t => null)
                .WithMany()
                .HasForeignKey(t => t.BatteryMakeCode)
                .WillCascadeOnDelete(false);
        }
    }
}
