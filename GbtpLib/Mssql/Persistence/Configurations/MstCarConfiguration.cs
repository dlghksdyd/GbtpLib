using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstCarConfiguration : EntityTypeConfiguration<MstCarEntity>
    {
        public MstCarConfiguration()
        {
            ToTable("MST_CAR");
            HasKey(x => new { x.CarMakeCode, x.CarCode });
        }
    }
}
