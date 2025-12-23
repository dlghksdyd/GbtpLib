using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfWmsCmdDataHistConfiguration : EntityTypeConfiguration<ItfWmsCmdDataHistEntity>
    {
        public ItfWmsCmdDataHistConfiguration()
        {
            ToTable("ITF_WMS_CMD_DATA_HIST");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
