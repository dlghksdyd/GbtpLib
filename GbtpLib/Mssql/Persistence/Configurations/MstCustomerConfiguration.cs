using System.Data.Entity.ModelConfiguration;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Persistence.Configurations
{
    public class MstCustomerConfiguration : EntityTypeConfiguration<MstCustomerEntity>
    {
        public MstCustomerConfiguration()
        {
            ToTable("MST_CUSTOMER");
            HasKey(x => x.CustomerCode);
        }
    }
}
