using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfWmsCmdDataConfiguration : EntityTypeConfiguration<ItfWmsCmdDataEntity>
    {
        public ItfWmsCmdDataConfiguration()
        {
            ToTable("ITF_WMS_CMD_DATA");
            HasKey(x => new { x.IfUid, x.IfDate, x.CommandCode });
        }
    }
}
