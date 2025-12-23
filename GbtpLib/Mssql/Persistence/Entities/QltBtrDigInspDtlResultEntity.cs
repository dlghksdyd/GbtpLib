using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("QLT_BTR_DIG_INSP_DTL_RESULT")]
    public class QltBtrDigInspDtlResultEntity
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
        [Key, Column("DIAG_ITEM_CD", Order = 9)] public string DiagItemCode { get; set; }
        [Key, Column("STEP_NO", Order = 10)] public string StepNo { get; set; }
        [Key, Column("COLT_DIAG_NM", Order = 11)] public string CollectDiagName { get; set; }

        [Column("COLT_VALUE")] public string CollectValue { get; set; }
        [Column("REG_DTM")] public DateTime? RegDateTime { get; set; }
    }
}
