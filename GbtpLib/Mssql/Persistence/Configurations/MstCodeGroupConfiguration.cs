using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstCodeGroupConfiguration : EntityTypeConfiguration<MstCodeGroupEntity>
    {
        public MstCodeGroupConfiguration()
        {
            ToTable("MST_CODE_GROUP");
            HasKey(x => x.CodeGroup);
        }
    }
}
