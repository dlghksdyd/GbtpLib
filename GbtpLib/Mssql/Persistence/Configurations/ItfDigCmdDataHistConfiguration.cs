using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfDigCmdDataHistConfiguration : EntityTypeConfiguration<ItfDigCmdDataHistEntity>
    {
        public ItfDigCmdDataHistConfiguration()
        {
            ToTable("ITF_DIG_CMD_DATA_HIST");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
