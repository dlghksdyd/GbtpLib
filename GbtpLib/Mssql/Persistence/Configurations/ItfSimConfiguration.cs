using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class ItfSimConfiguration : EntityTypeConfiguration<ItfSimEntity>
    {
        public ItfSimConfiguration()
        {
            ToTable("ITF_SIM");
            HasKey(x => new { x.SystemCode, x.ScenarioCode, x.ScenarioCommandCode, x.ScenarioStatusCode });
        }
    }
}
