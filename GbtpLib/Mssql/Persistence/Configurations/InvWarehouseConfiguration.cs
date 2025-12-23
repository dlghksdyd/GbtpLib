using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class InvWarehouseConfiguration : EntityTypeConfiguration<InvWarehouseEntity>
    {
        public InvWarehouseConfiguration()
        {
            ToTable("INV_WAREHOUSE");
            HasKey(x => new { x.SiteCode, x.FactoryCode, x.WarehouseCode, x.Row, x.Col, x.Level });
        }
    }
}
