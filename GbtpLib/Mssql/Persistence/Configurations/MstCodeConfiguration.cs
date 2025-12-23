using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstCodeConfiguration : EntityTypeConfiguration<MstCodeEntity>
    {
        public MstCodeConfiguration()
        {
            ToTable("MST_CODE");
            HasKey(x => new { x.CodeGroup, x.Code });
        }
    }
}
