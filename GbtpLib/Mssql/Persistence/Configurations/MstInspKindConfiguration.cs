using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstInspKindConfiguration : EntityTypeConfiguration<MstInspKindEntity>
    {
        public MstInspKindConfiguration()
        {
            ToTable("MST_INSP_KIND");
            HasKey(x => new { x.InspKindGroupCode, x.InspKindCode });
        }
    }
}
