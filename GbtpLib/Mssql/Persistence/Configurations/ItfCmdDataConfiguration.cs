using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfCmdDataConfiguration : EntityTypeConfiguration<ItfCmdDataEntity>
    {
        public ItfCmdDataConfiguration()
        {
            ToTable("ITF_CMD_DATA");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
