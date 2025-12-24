using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_CAR")]
    public class MstCarEntity
    {
        [Key, Column("CAR_MAKE_CD", Order = 0)]
        [MaxLength(2)]
        public string CarMakeCode { get; set; }

        [Key, Column("CAR_CD", Order = 1)]
        [MaxLength(2)]
        public string CarCode { get; set; }

        [Column("CAR_NM")]
        [MaxLength(40)]
        public string CarName { get; set; }

        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }

        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }

        [Column("RELS_YEAR")]
        [MaxLength(4)]
        public string ReleaseYear { get; set; }

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

        // Navigation properties
        public virtual MstCarMakeEntity CarMake { get; set; }
    }
}
