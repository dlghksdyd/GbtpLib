using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfSysConCheckConfiguration : EntityTypeConfiguration<ItfSysConCheckEntity>
    {
        public ItfSysConCheckConfiguration()
        {
            ToTable("ITF_SYS_CON_CHECK");
            HasKey(x => x.SystemCode);
        }
    }
}
