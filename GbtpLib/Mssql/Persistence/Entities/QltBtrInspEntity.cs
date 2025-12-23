using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("QLT_BTR_INSP")]
    public class QltBtrInspEntity
    {
        [Key, Column("SITE_CD", Order = 0)] public string SiteCode { get; set; }
        [Key, Column("FACT_CD", Order = 1)] public string FactoryCode { get; set; }
        [Key, Column("PRCS_CD", Order = 2)] public string ProcessCode { get; set; }
        [Key, Column("MC_CD", Order = 3)] public string MachineCode { get; set; }
        [Key, Column("MC_CH", Order = 4)] public string MachineChannel { get; set; }
        [Key, Column("INSP_KIND_GROUP_CD", Order = 5)] public string InspKindGroupCode { get; set; }
        [Key, Column("INSP_KIND_CD", Order = 6)] public string InspKindCode { get; set; }
        [Key, Column("LBL_ID", Order = 7)] public string LabelId { get; set; }
        [Key, Column("INSP_SEQ", Order = 8)] public string InspectSeq { get; set; }

        [Column("INSP_STS")] public string InspectStatus { get; set; }
        [Column("BTR_DIAG_STS")] public string BatteryDiagStatus { get; set; }
        [Column("VOLT_VALUE")] public decimal? VoltValue { get; set; }
        [Column("CURR_VALUE")] public decimal? CurrValue { get; set; }
        [Column("INSP_STA_DT")] public DateTime? InspectStart { get; set; }
        [Column("INSP_END_DT")] public DateTime? InspectEnd { get; set; }
        [Column("INSP_GRD")] public string InspectGrade { get; set; }
        [Column("NOTE")] public string Note { get; set; }
    }
}
