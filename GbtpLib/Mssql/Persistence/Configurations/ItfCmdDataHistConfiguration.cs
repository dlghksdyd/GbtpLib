using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfCmdDataHistConfiguration : EntityTypeConfiguration<ItfCmdDataHistEntity>
    {
        public ItfCmdDataHistConfiguration()
        {
            ToTable("ITF_CMD_DATA_HIST");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
