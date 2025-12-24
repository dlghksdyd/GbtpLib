using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_BTR_TYPE")]
    public class MstBtrTypeEntity
    {
        [Key]
        [Column("BTR_TYPE_NO")]
        public int BatteryTypeNo { get; set; }

        [Column("CAR_MAKE_CD")]
        [MaxLength(2)]
        public string CarMakeCode { get; set; }

        [Column("CAR_CD")]
        [MaxLength(2)]
        public string CarCode { get; set; }

        [Column("BTR_MAKE_CD")]
        [MaxLength(3)]
        public string BatteryMakeCode { get; set; }

        [Column("BTR_TYPE_SLT_CD")]
        [MaxLength(3)]
        public string BatteryTypeSelectCode { get; set; }

        [Column("BTR_TYPE_CD")]
        [MaxLength(35)]
        public string BatteryTypeCode { get; set; }

        [Column("BTR_TYPE_NM")]
        [MaxLength(40)]
        public string BatteryTypeName { get; set; }

        [Column("CAR_RELS_YEAR")]
        [MaxLength(4)]
        public string CarReleaseYear { get; set; }

        [Column("PACK_MDLE_CD")]
        [MaxLength(10)]
        public string PackModuleCode { get; set; }

        [Column("CAPA_VALUE")]
        public decimal? CapaValue { get; set; }
        [Column("NOMINL_VALUE")]
        public decimal? NominlValue { get; set; }
        [Column("CHARGE_VALUE")]
        public decimal? ChargeValue { get; set; }
        [Column("DISCRG_VALUE")]
        public decimal? DischargeValue { get; set; }
        [Column("VOLT_MAX_VALUE")]
        public decimal? VoltMaxValue { get; set; }
        [Column("VOLT_MIN_VALUE")]
        public decimal? VoltMinValue { get; set; }
        [Column("ACIR_MAX_VALUE")]
        public decimal? AcirMaxValue { get; set; }
        [Column("DCIR_MAX_VALUE")]
        public decimal? DcirMaxValue { get; set; }
        [Column("INSUL_MIN_VALUE")]
        public decimal? InsulMinValue { get; set; }

        [Column("CELL_QTY")]
        [MaxLength(10)]
        public string CellQty { get; set; }
        [Column("BTR_TYPE_NM_CELL")]
        [MaxLength(40)]
        public string BatteryTypeNameCell { get; set; }
        [Column("CONN_TYPE")]
        [MaxLength(10)]
        public string ConnType { get; set; }
        [Column("IMAGE_NM")]
        [MaxLength(40)]
        public string ImageName { get; set; }
        [Column("IMAGE_FILE")]
        public byte[] ImageFile { get; set; }
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

        // Navigation properties
        public virtual MstCarMakeEntity CarMake { get; set; }
        public virtual MstCarEntity Car { get; set; }
        public virtual MstBtrMakeEntity BatteryMake { get; set; }
    }
}
