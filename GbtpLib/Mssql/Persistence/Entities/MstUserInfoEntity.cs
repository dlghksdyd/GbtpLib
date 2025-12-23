using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("MST_USER_INFO")]
    public class MstUserInfoEntity
    {
        [Key, Column("SITE_CD", Order = 0)]
        [MaxLength(2)]
        public string SiteCode { get; set; }

        [Key, Column("USER_ID", Order = 1)]
        [MaxLength(10)]
        public string UserId { get; set; }

        [Column("USER_GROUP_CD")]
        [MaxLength(10)]
        public string UserGroupCode { get; set; }

        [Column("USER_NM")]
        [MaxLength(40)]
        public string UserName { get; set; }

        [Column("DEPT_CD")]
        [MaxLength(10)]
        public string DepartmentCode { get; set; }

        [Column("PWD")]
        [MaxLength(100)]
        public string Password { get; set; }

        [Column("ENTRY_DAT")]
        [MaxLength(8)]
        public string EntryDate { get; set; }

        [Column("RETIRED_DAT")]
        [MaxLength(8)]
        public string RetiredDate { get; set; }

        [Column("SEX")]
        [MaxLength(1)]
        public string Sex { get; set; }

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
