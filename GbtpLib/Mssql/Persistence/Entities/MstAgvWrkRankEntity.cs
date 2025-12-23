using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_AGV_WRK_RANK")]
    public class MstAgvWrkRankEntity
    {
        [Key]
        [Column("WRK_CD")]
        [MaxLength(10)]
        public string WorkCode { get; set; }

        [Column("WRK_SEQ")]
        public int? WorkSeq { get; set; }
        [Column("WRK_NM")]
        [MaxLength(40)]
        public string WorkName { get; set; }
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
