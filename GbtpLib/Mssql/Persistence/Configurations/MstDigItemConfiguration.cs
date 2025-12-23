using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstDigItemConfiguration : EntityTypeConfiguration<MstDigItemEntity>
    {
        public MstDigItemConfiguration()
        {
            ToTable("MST_DIG_ITEM");
            HasKey(x => x.DiagItemCode);
        }
    }
}
