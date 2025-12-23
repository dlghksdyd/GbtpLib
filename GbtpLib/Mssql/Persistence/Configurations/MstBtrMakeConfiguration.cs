using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstBtrMakeConfiguration : EntityTypeConfiguration<MstBtrMakeEntity>
    {
        public MstBtrMakeConfiguration()
        {
            ToTable("MST_BTR_MAKE");
            HasKey(x => x.BatteryMakeCode);
        }
    }
}
