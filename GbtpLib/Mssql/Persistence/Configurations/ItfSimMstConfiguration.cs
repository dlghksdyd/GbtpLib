using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfSimMstConfiguration : EntityTypeConfiguration<ItfSimMstEntity>
    {
        public ItfSimMstConfiguration()
        {
            ToTable("ITF_SIM_MST");
            HasKey(x => new { x.SystemCode, x.ScenarioCode, x.ScenarioCommandCode });
        }
    }
}
