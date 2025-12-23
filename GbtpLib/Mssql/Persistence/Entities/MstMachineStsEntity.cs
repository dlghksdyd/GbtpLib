using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_MACHINE_STS")]
    public class MstMachineStsEntity
    {
        [Key]
        [Column("STS_CD")]
        [MaxLength(10)]
        public string StatusCode { get; set; }

        [Column("STS_NM")]
        [MaxLength(100)]
        public string StatusName { get; set; }
        [Column("STS_TYPE")]
        [MaxLength(10)]
        public string StatusType { get; set; }
        [Column("MC_TYPE")]
        [MaxLength(10)]
        public string MachineType { get; set; }
        [Column("NOTE")]
        [MaxLength(255)]
        public string Note { get; set; }
        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }
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
        [Column("ARM_FLAG")]
        [MaxLength(1)]
        public string ArmFlag { get; set; }
    }
}
