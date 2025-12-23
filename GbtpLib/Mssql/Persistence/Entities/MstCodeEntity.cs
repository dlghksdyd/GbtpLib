using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_CODE")]
    public class MstCodeEntity
    {
        [Key, Column("COMM_CD_GROUP", Order = 0)]
        [MaxLength(10)]
        public string CodeGroup { get; set; }
        [Key, Column("COMM_CD", Order = 1)]
        [MaxLength(10)]
        public string Code { get; set; }

        [Column("COMM_CD_NM")]
        [MaxLength(40)]
        public string CodeName { get; set; }
        [Column("COMM_CD_NM_LANG1")]
        [MaxLength(40)]
        public string CodeNameLang1 { get; set; }
        [Column("COMM_CD_NM_LANG2")]
        public string CodeNameLang2 { get; set; }
        [Column("COMM_CD_NM_LANG3")]
        [MaxLength(40)]
        public string CodeNameLang3 { get; set; }
        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }
        [Column("REMARK")]
        [MaxLength(500)]
        public string Remark { get; set; }
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
