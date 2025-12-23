using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstBtrTypeConfiguration : EntityTypeConfiguration<MstBtrTypeEntity>
    {
        public MstBtrTypeConfiguration()
        {
            ToTable("MST_BTR_TYPE");
            HasKey(x => x.BatteryTypeNo);
        }
    }
}
