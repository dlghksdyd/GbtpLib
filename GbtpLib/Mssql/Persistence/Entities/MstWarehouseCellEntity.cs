using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_WAREHOUSE_CELL")]
    public class MstWarehouseCellEntity
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

        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }
        [Column("REG_ID")]
        [MaxLength(10)]
        public string RegId { get; set; }
        [Column("REG_DTM")]
        public DateTime? RegDateTime { get; set; }
        [Column("MOD_ID")]
        [MaxLength(10)]
        public string ModId { get; set; }
        [Column("MOD_DTM")]
        public DateTime? ModDateTime { get; set; }
    }
}
