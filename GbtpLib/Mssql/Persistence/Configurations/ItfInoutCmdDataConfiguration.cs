using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfInoutCmdDataConfiguration : EntityTypeConfiguration<ItfInoutCmdDataEntity>
    {
        public ItfInoutCmdDataConfiguration()
        {
            ToTable("ITF_INOUT_CMD_DATA");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
