using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class InvWarehouseHistConfiguration : EntityTypeConfiguration<InvWarehouseHistEntity>
    {
        public InvWarehouseHistConfiguration()
        {
            ToTable("INV_WAREHOUSE_HIST");
            HasKey(x => new { x.SiteCode, x.FactoryCode, x.WarehouseCode, x.Row, x.Col, x.Level, x.SaveDate, x.HistSeq });
        }
    }
}
