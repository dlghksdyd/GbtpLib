using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("SYS_MENU_MST")]
    public class SysMenuMstEntity
    {
        [Key, Column("SITE_CD", Order = 0)]
        [MaxLength(2)]
        public string SiteCode { get; set; }
        [Key, Column("SYSTEM_ID", Order = 1)]
        [MaxLength(3)]
        public string SystemId { get; set; }
        [Key, Column("PG_ID", Order = 2)]
        [MaxLength(15)]
        public string PageId { get; set; }

        [Column("UP_PG_ID")]
        [MaxLength(15)]
        public string UpPageId { get; set; }
        [Column("SUB_SYSTEM_ID")]
        [MaxLength(15)]
        public string SubSystemId { get; set; }
        [Column("PG_NM")]
        [MaxLength(40)]
        public string PageName { get; set; }
        [Column("PG_NM_LANG1")]
        [MaxLength(40)]
        public string PageNameLang1 { get; set; }
        [Column("PG_NM_LANG2")]
        [MaxLength(40)]
        public string PageNameLang2 { get; set; }
        [Column("PG_NM_LANG3")]
        [MaxLength(40)]
        public string PageNameLang3 { get; set; }
        [Column("REMARK")]
        [MaxLength(500)]
        public string Remark { get; set; }
        [Column("LIST_ORDER")]
        public int? ListOrder { get; set; }
        [Column("TRAN_YN")]
        [MaxLength(1)]
        public string TranYn { get; set; }
        [Column("MENU_YN")]
        [MaxLength(1)]
        public string MenuYn { get; set; }
        [Column("USE_YN")]
        [MaxLength(1)]
        public string UseYn { get; set; }
        [Column("OPEN_TYPE")]
        [MaxLength(30)]
        public string OpenType { get; set; }
        [Column("ASSEMBLY_NM")]
        [MaxLength(100)]
        public string AssemblyName { get; set; }
        [Column("NAMESPACE")]
        [MaxLength(100)]
        public string Namespace { get; set; }
        [Column("FORM_NM")]
        [MaxLength(100)]
        public string FormName { get; set; }
        [Column("DEV_YN")]
        [MaxLength(1)]
        public string DevYn { get; set; }
        [Column("NEW_FLAG")]
        [MaxLength(1)]
        public string NewFlag { get; set; }
        [Column("VIEW_FLAG")]
        [MaxLength(1)]
        public string ViewFlag { get; set; }
        [Column("MOD_FLAG")]
        [MaxLength(1)]
        public string ModFlag { get; set; }
        [Column("DEL_FLAG")]
        [MaxLength(1)]
        public string DelFlag { get; set; }
        [Column("PRT_FLAG")]
        [MaxLength(1)]
        public string PrtFlag { get; set; }
        [Column("XLS_FLAG")]
        [MaxLength(1)]
        public string XlsFlag { get; set; }
        [Column("IMAGE_IDX")]
        [MaxLength(20)]
        public string ImageIndex { get; set; }
        [Column("MENU_DEPTH")]
        [MaxLength(1)]
        public string MenuDepth { get; set; }
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
