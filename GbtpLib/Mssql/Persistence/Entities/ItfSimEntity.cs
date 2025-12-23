using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("ITF_SIM")]
    public class ItfSimEntity
    {
        [Key, Column("SNRO_SYS_CD", Order = 0)]
        [MaxLength(10)]
        public string SystemCode { get; set; }
        [Key, Column("SNRO_CD", Order = 1)]
        [MaxLength(20)]
        public string ScenarioCode { get; set; }
        [Key, Column("SNRO_CMD_CD", Order = 2)]
        [MaxLength(10)]
        public string ScenarioCommandCode { get; set; }
        [Key, Column("SNRO_STS_CD", Order = 3)]
        [MaxLength(10)]
        public string ScenarioStatusCode { get; set; }

        [Column("SNRO_NM")]
        [MaxLength(100)]
        public string ScenarioName { get; set; }
        [Column("SNRO_SEQ")]
        public int? ScenarioSeq { get; set; }
        [Column("SNRO_BTR_STS")]
        [MaxLength(10)]
        public string BatteryStatus { get; set; }
        [Column("SNRO_STS_ING")]
        [MaxLength(1)]
        public string StatusIng { get; set; }
        [Column("SNRO_END_FLAG")]
        [MaxLength(1)]
        public string EndFlag { get; set; }
    }
}
