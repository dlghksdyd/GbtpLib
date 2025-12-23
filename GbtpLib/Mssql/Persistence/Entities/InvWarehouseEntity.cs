using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("INV_WAREHOUSE")]
    public class InvWarehouseEntity
    {
        [Key, Column("SITE_CD", Order = 0)]
        [MaxLength(2)]
        public string SiteCode { get; set; }

        [Key, Column("FACT_CD", Order = 1)]
        [MaxLength(4)]
        public string FactoryCode { get; set; }

        [Key, Column("WH_CD", Order = 2)]
        [MaxLength(4)]
        public string WarehouseCode { get; set; }

        [Key, Column("ROW", Order = 3)]
        [MaxLength(3)]
        public string Row { get; set; }

        [Key, Column("COL", Order = 4)]
        [MaxLength(3)]
        public string Col { get; set; }

        [Key, Column("LVL", Order = 5)]
        [MaxLength(3)]
        public string Level { get; set; }

        [Column("LBL_ID")]
        [MaxLength(35)]
        public string LabelId { get; set; }

        [Column("LOAD_GRD")]
        [MaxLength(1)]
        public string LoadGrade { get; set; }

        [Column("STORE_DIV")]
        [MaxLength(10)]
        public string StoreDiv { get; set; }

        [Column("STS")]
        [MaxLength(1)]
        public string Status { get; set; }

        [Column("NOTE")]
        [MaxLength(255)]
        public string Note { get; set; }
    }
}
