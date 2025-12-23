using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("ITF_CMD_DATA")]
    public class ItfCmdDataEntity
    {
        [Key, Column("IF_UID", Order = 0)]
        [MaxLength(20)]
        public string IfUid { get; set; }

        [Key, Column("IF_DATE", Order = 1)]
        [MaxLength(8)]
        public string IfDate { get; set; }

        [Key, Column("CMD_CD", Order = 2)]
        [MaxLength(4)]
        public string CommandCode { get; set; }

        [Column("DATA1")]
        [MaxLength(100)]
        public string Data1 { get; set; }
        [Column("DATA2")]
        [MaxLength(100)]
        public string Data2 { get; set; }
        [Column("DATA3")]
        [MaxLength(100)]
        public string Data3 { get; set; }
        [Column("DATA4")]
        [MaxLength(100)]
        public string Data4 { get; set; }
        [Column("DATA5")]
        [MaxLength(100)]
        public string Data5 { get; set; }
        [Column("DATA6")]
        [MaxLength(100)]
        public string Data6 { get; set; }
        [Column("DATA7")]
        [MaxLength(100)]
        public string Data7 { get; set; }
        [Column("DATA8")]
        [MaxLength(100)]
        public string Data8 { get; set; }
        [Column("DATA9")]
        [MaxLength(100)]
        public string Data9 { get; set; }
        [Column("DATA10")]
        [MaxLength(100)]
        public string Data10 { get; set; }

        [Column("IF_FLAG")]
        [MaxLength(1)]
        public string IfFlag { get; set; }

        [Column("REQ_TIME")]
        public DateTime? RequestTime { get; set; }
        [Column("RES_TIME")]
        public DateTime? ResponseTime { get; set; }

        [Column("REQ_SYS")]
        [MaxLength(10)]
        public string RequestSystem { get; set; }
        [Column("RES_SYS")]
        [MaxLength(10)]
        public string ResponseSystem { get; set; }
    }
}
