using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_CAR_MAKE")]
    public class MstCarMakeEntity
    {
        [Key]
        [Column("CAR_MAKE_CD")]
        [MaxLength(2)]
        public string CarMakeCode { get; set; }

        [Column("CAR_MAKE_NM")]
        [MaxLength(40)]
        public string CarMakeName { get; set; }

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
