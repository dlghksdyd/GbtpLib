using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GbtpLib.Mssql.Persistence.Entities
{
    [Table("SYS_GRID")]
    public class SysGridEntity
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
        [Key, Column("PG_SEQ", Order = 3)]
        [MaxLength(15)]
        public string PageSeq { get; set; }
        [Key, Column("CLM_SEQ", Order = 4)]
        public int ColumnSeq { get; set; }

        [Column("TABLE_NAME")]
        [MaxLength(100)]
        public string TableName { get; set; }
        [Column("COL_NAME")]
        [MaxLength(100)]
        public string ColumnName { get; set; }
        [Column("HEAD_DESC1")]
        [MaxLength(100)]
        public string HeadDesc1 { get; set; }
        [Column("HEAD_DESC2")]
        [MaxLength(100)]
        public string HeadDesc2 { get; set; }
        [Column("HEAD_DESC3")]
        [MaxLength(100)]
        public string HeadDesc3 { get; set; }
        [Column("HEAD_DESC4")]
        [MaxLength(100)]
        public string HeadDesc4 { get; set; }
        [Column("HEAD_DESC_EN1")]
        [MaxLength(100)]
        public string HeadDescEn1 { get; set; }
        [Column("HEAD_DESC_EN2")]
        [MaxLength(100)]
        public string HeadDescEn2 { get; set; }
        [Column("HEAD_DESC_EN3")]
        [MaxLength(100)]
        public string HeadDescEn3 { get; set; }
        [Column("HEAD_DESC_EN4")]
        [MaxLength(100)]
        public string HeadDescEn4 { get; set; }
        [Column("HEAD_DESC_CH1")]
        public string HeadDescCh1 { get; set; }
        [Column("HEAD_DESC_CH2")]
        public string HeadDescCh2 { get; set; }
        [Column("HEAD_DESC_CH3")]
        public string HeadDescCh3 { get; set; }
        [Column("HEAD_DESC_CH4")]
        public string HeadDescCh4 { get; set; }
        [Column("DISPLAY_SEQ")]
        public int? DisplaySeq { get; set; }
        [Column("WIDTH")]
        [MaxLength(3)]
        public string Width { get; set; }
        [Column("LOCK_YN")]
        [MaxLength(1)]
        public string LockYn { get; set; }
        [Column("VISIBLE_YN")]
        [MaxLength(1)]
        public string VisibleYn { get; set; }
        [Column("AUTOSORT_YN")]
        [MaxLength(1)]
        public string AutoSortYn { get; set; }
        [Column("HALIGN")]
        [MaxLength(10)]
        public string HAlign { get; set; }
        [Column("VALIGN")]
        [MaxLength(10)]
        public string VAlign { get; set; }
        [Column("MAXROW")]
        [MaxLength(10)]
        public string MaxRow { get; set; }
        [Column("FROZEN_COL")]
        [MaxLength(1)]
        public string FrozenCol { get; set; }
        [Column("FROZEN_ROW")]
        [MaxLength(1)]
        public string FrozenRow { get; set; }
        [Column("QUERY")]
        [MaxLength(200)]
        public string Query { get; set; }
        [Column("PARAM_LIST")]
        [MaxLength(1000)]
        public string ParamList { get; set; }
        [Column("CHECK_YN")]
        [MaxLength(1)]
        public string CheckYn { get; set; }
        [Column("ROUND")]
        [MaxLength(10)]
        public string Round { get; set; }
        [Column("REMARK")]
        [MaxLength(50)]
        public string Remark { get; set; }
        [Column("FILTER")]
        [MaxLength(50)]
        public string Filter { get; set; }
        [Column("BEST_FIT")]
        [MaxLength(1)]
        public string BestFit { get; set; }
        [Column("BACK_COLOR")]
        [MaxLength(30)]
        public string BackColor { get; set; }
        [Column("FORE_COLOR")]
        [MaxLength(30)]
        public string ForeColor { get; set; }
        [Column("CELL_TYPE")]
        [MaxLength(30)]
        public string CellType { get; set; }
        [Column("DISPLAY_MEMBER")]
        [MaxLength(30)]
        public string DisplayMember { get; set; }
        [Column("VALUE_MEMBER")]
        [MaxLength(30)]
        public string ValueMember { get; set; }
        [Column("SRCH_TYPE")]
        [MaxLength(30)]
        public string SearchType { get; set; }
        [Column("SRCH_SEQ")]
        public int? SearchSeq { get; set; }
        [Column("SRCH_YN")]
        [MaxLength(1)]
        public string SearchYn { get; set; }
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
