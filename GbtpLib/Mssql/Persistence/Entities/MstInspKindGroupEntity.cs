using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_INSP_KIND_GROUP")]
    public class MstInspKindGroupEntity
    {
        [Key]
        [Column("INSP_KIND_GROUP_CD")]
        [MaxLength(2)]
        public string InspKindGroupCode { get; set; }

        [Column("INSP_KIND_GROUP_NM")]
        [MaxLength(40)]
        public string InspKindGroupName { get; set; }
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
