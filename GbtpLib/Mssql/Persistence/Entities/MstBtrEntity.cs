using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_BTR")]
    public class MstBtrEntity
    {
        [Key]
        [Column("LBL_ID")]
        [MaxLength(35)]
        public string LabelId { get; set; }

        [Column("BTR_TYPE_NO")]
        public int BatteryTypeNo { get; set; }

        [Column("PACK_MDLE_CD")]
        [MaxLength(3)]
        public string PackModuleCode { get; set; }

        [Column("SITE_CD")]
        [MaxLength(2)]
        public string SiteCode { get; set; }

        [Column("COLT_DAT")]
        [MaxLength(8)]
        public string CollectDate { get; set; }

        [Column("COLT_RESN")]
        [MaxLength(3)]
        public string CollectReason { get; set; }

        [Column("MUFT_DAT")]
        [MaxLength(8)]
        public string MuftDate { get; set; }

        [Column("MILE")]
        public int? Mile { get; set; }

        [Column("VER")]
        public int? Version { get; set; }

        [Column("NOTE")]
        [MaxLength(255)]
        public string Note { get; set; }

        [Column("PRT_YN")]
        [MaxLength(1)]
        public string PrintYn { get; set; }

        [Column("BTR_STS")]
        [MaxLength(2)]
        public string BatteryStatus { get; set; }

        [Column("STO_INSP_FLAG")]
        [MaxLength(1)]
        public string StoreInspFlag { get; set; }

        [Column("STO_INSP_END_DTM")]
        public DateTime? StoreInspEndTime { get; set; }

        [Column("ENE_INSP_FLAG")]
        [MaxLength(1)]
        public string EnergyInspFlag { get; set; }

        [Column("ENE_INSP_CNT")]
        public int? EnergyInspCount { get; set; }

        [Column("ENE_INSP_END_DTM")]
        public DateTime? EnergyInspEndTime { get; set; }

        [Column("DIG_INSP_FLAG")]
        [MaxLength(1)]
        public string DigInspFlag { get; set; }

        [Column("DIG_INSP_END_DTM")]
        public DateTime? DigInspEndTime { get; set; }

        [Column("EMG_OUT_FLAG")]
        [MaxLength(1)]
        public string EmergencyOutFlag { get; set; }

        [Column("EMG_OUT_NOTE")]
        [MaxLength(255)]
        public string EmergencyOutNote { get; set; }

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
