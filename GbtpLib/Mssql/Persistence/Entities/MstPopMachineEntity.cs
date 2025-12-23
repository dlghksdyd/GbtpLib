using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_POP_MACHINE")]
    public class MstPopMachineEntity
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
        [Key, Column("MC_CD", Order = 3)]
        [MaxLength(4)]
        public string MachineCode { get; set; }

        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }
        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }
        [Column("NOTE")]
        [MaxLength(255)]
        public string Note { get; set; }
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
