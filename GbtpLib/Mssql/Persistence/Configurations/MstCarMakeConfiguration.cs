using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstCarMakeConfiguration : EntityTypeConfiguration<MstCarMakeEntity>
    {
        public MstCarMakeConfiguration()
        {
            ToTable("MST_CAR_MAKE");
            HasKey(x => x.CarMakeCode);
        }
    }
}
