using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstDeptConfiguration : EntityTypeConfiguration<MstDeptEntity>
    {
        public MstDeptConfiguration()
        {
            ToTable("MST_DEPT");
            HasKey(x => new { x.SiteCode, x.DeptCode });
        }
    }
}
