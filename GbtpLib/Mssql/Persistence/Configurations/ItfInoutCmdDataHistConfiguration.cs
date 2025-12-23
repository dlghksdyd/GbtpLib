using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfInoutCmdDataHistConfiguration : EntityTypeConfiguration<ItfInoutCmdDataHistEntity>
    {
        public ItfInoutCmdDataHistConfiguration()
        {
            ToTable("ITF_INOUT_CMD_DATA_HIST");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
