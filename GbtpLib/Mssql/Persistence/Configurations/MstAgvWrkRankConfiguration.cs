using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstAgvWrkRankConfiguration : EntityTypeConfiguration<MstAgvWrkRankEntity>
    {
        public MstAgvWrkRankConfiguration()
        {
            ToTable("MST_AGV_WRK_RANK");
            HasKey(x => x.WorkCode);
        }
    }
}
