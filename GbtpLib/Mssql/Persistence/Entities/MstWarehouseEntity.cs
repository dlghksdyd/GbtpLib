using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_WAREHOUSE")]
    public class MstWarehouseEntity
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

        [Column("WH_NM")]
        [MaxLength(40)]
        public string WarehouseName { get; set; }
        [Column("WH_TYPE")]
        [MaxLength(10)]
        public string WarehouseType { get; set; }
        [Column("WH_DIV")]
        [MaxLength(10)]
        public string WarehouseDiv { get; set; }
        [Column("WH_SVR_LNK")]
        [MaxLength(40)]
        public string WarehouseServerLink { get; set; }
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
