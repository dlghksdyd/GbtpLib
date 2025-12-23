using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfDigCmdDataConfiguration : EntityTypeConfiguration<ItfDigCmdDataEntity>
    {
        public ItfDigCmdDataConfiguration()
        {
            ToTable("ITF_DIG_CMD_DATA");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
