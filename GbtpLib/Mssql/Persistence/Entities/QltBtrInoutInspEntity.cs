using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("QLT_BTR_INOUT_INSP")]
    public class QltBtrInoutInspEntity
    {
        [Key, Column("SITE_CD", Order = 0)] public string SiteCode { get; set; }
        [Key, Column("FACT_CD", Order = 1)] public string FactoryCode { get; set; }
        [Key, Column("PRCS_CD", Order = 2)] public string ProcessCode { get; set; }
        [Key, Column("MC_CD", Order = 3)] public string MachineCode { get; set; }
        [Key, Column("INSP_KIND_GROUP_CD", Order = 4)] public string InspKindGroupCode { get; set; }
        [Key, Column("INSP_KIND_CD", Order = 5)] public string InspKindCode { get; set; }
        [Key, Column("LBL_ID", Order = 6)] public string LabelId { get; set; }
        [Key, Column("INSP_SEQ", Order = 7)] public string InspectSeq { get; set; }

        [Column("INSP_VAL")] public string InspectValue { get; set; }
        [Column("INSP_RESULT")] public string InspectResult { get; set; }
        [Column("INSP_STA_DT")] public DateTime? InspectStart { get; set; }
        [Column("INSP_END_DT")] public DateTime? InspectEnd { get; set; }
        [Column("NOTE")] public string Note { get; set; }
        [Column("REG_ID")] public string RegId { get; set; }
        [Column("REG_DTM")] public DateTime? RegDateTime { get; set; }
        [Column("MOD_ID")] public string ModId { get; set; }
        [Column("MOD_DTM")] public DateTime? ModDateTime { get; set; }
    }
}
