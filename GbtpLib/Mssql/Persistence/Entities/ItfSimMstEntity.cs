using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("ITF_SIM_MST")]
    public class ItfSimMstEntity
    {
        [Key, Column("SNRO_SYS_CD", Order = 0)]
        [MaxLength(10)]
        public string SystemCode { get; set; }
        [Key, Column("SNRO_CD", Order = 1)]
        [MaxLength(20)]
        public string ScenarioCode { get; set; }
        [Key, Column("SNRO_CMD_CD", Order = 2)]
        [MaxLength(20)]
        public string ScenarioCommandCode { get; set; }

        [Column("SNRO_NM")]
        [MaxLength(100)]
        public string ScenarioName { get; set; }
    }
}
