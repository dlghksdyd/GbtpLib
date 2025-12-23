using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstFactoryConfiguration : EntityTypeConfiguration<MstFactoryEntity>
    {
        public MstFactoryConfiguration()
        {
            ToTable("MST_FACTORY");
            HasKey(x => new { x.SiteCode, x.FactoryCode });
        }
    }
}
