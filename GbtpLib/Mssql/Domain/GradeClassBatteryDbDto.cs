namespace GbtpLib.Mssql.Domain
{
    /// <summary>
    /// DTO for grade-class battery search results used across application and UI layers.
    /// Mirrors fields selected from INV_WAREHOUSE, QLT_BTR_INSP, MST_SITE, MST_BTR, MST_BTR_TYPE, MST_CAR_MAKE, MST_CAR, MST_BTR_MAKE.
    /// </summary>
    public class GradeClassBatteryDbDto
    {
        public int ROW { get; set; }
        public int COL { get; set; }
        public int LVL { get; set; }
        public string LBL_ID { get; set; }
        public string INSP_GRD { get; set; }
        public string SITE_NM { get; set; }
        public string COLT_DAT { get; set; }
        public string COLT_RESN { get; set; }
        public string PACK_MDLE_CD { get; set; }
        public string BTR_TYPE_NM { get; set; }
        public string CAR_RELS_YEAR { get; set; }
        public string CAR_MAKE_NM { get; set; }
        public string CAR_NM { get; set; }
        public string BTR_MAKE_NM { get; set; }
    }
}
