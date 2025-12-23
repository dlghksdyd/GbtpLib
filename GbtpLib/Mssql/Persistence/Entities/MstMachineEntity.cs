using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_MACHINE")]
    public class MstMachineEntity
    {
        [Key, Column("SITE_CD", Order = 0)]
        [MaxLength(2)]
        public string SiteCode { get; set; }
        [Key, Column("FACT_CD", Order = 1)]
        [MaxLength(4)]
        public string FactoryCode { get; set; }
        [Key, Column("MC_CD", Order = 2)]
        [MaxLength(4)]
        public string MachineCode { get; set; }

        [Column("MC_NM")]
        [MaxLength(40)]
        public string MachineName { get; set; }
        [Column("MC_TYPE")]
        [MaxLength(10)]
        public string MachineType { get; set; }
        [Column("POP_CD")]
        [MaxLength(6)]
        public string PopCode { get; set; }
        [Column("MC_DTL")]
        [MaxLength(100)]
        public string MachineDetail { get; set; }
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
