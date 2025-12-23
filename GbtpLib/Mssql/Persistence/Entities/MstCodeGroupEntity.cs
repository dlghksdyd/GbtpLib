using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_CODE_GROUP")]
    public class MstCodeGroupEntity
    {
        [Key]
        [Column("COMM_CD_GROUP")]
        [MaxLength(10)]
        public string CodeGroup { get; set; }

        [Column("COMM_CD_GROUP_NM")]
        [MaxLength(40)]
        public string CodeGroupName { get; set; }
        [Column("COMM_CD_GROUP_NM_LANG1")]
        [MaxLength(40)]
        public string CodeGroupNameLang1 { get; set; }
        [Column("COMM_CD_GROUP_NM_LANG2")]
        public string CodeGroupNameLang2 { get; set; }
        [Column("COMM_CD_GROUP_NM_LANG3")]
        [MaxLength(40)]
        public string CodeGroupNameLang3 { get; set; }
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
    }
}
