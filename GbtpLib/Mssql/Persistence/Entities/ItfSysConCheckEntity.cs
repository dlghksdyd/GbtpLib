using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("ITF_SYS_CON_CHECK")]
    public class ItfSysConCheckEntity
    {
        [Key]
        [Column("SYS_CD")]
        [MaxLength(10)]
        public string SystemCode { get; set; }

        [Column("CON_FLAG")]
        [MaxLength(1)]
        public string ConnectionFlag { get; set; }

        [Column("RES_TIME")]
        public DateTime? ResponseTime { get; set; }
    }
}
