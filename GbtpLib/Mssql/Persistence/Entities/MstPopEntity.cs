using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_POP")]
    public class MstPopEntity
    {
        [Key, Column("SITE_CD", Order = 0)]
        [MaxLength(2)]
        public string SiteCode { get; set; }
        [Key, Column("FACT_CD", Order = 1)]
        [MaxLength(4)]
        public string FactoryCode { get; set; }
        [Key, Column("POP_CD", Order = 2)]
        [MaxLength(6)]
        public string PopCode { get; set; }

        [Column("POP_EQM_NM")]
        [MaxLength(40)]
        public string PopEqmName { get; set; }
        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }
        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }
        [Column("WRK_MODE")]
        [MaxLength(10)]
        public string WorkMode { get; set; }
        [Column("IP_ADDR")]
        [MaxLength(20)]
        public string IpAddress { get; set; }
        [Column("POP_CONN_YN")]
        [MaxLength(1)]
        public string PopConnYn { get; set; }
        [Column("POP_EQM_STS")]
        [MaxLength(1)]
        public string PopEqmStatus { get; set; }
        [Column("LBL_ID_STS")]
        [MaxLength(1)]
        public string LabelIdStatus { get; set; }
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
        [Column("RES_TIME")]
        public DateTime? ResTime { get; set; }
    }
}
