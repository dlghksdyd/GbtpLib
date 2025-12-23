namespace GbtpLib.Mssql.Domain.ReadModels
{
    public class SlotInfoDto
    {
        // INV_WAREHOUSE
        public int Row { get; set; }
        public int Col { get; set; }
        public int Level { get; set; }
        public string LabelId { get; set; }
        // QLT_BTR_INSP
        public string InspectGrade { get; set; }
        // MST_SITE
        public string SiteName { get; set; }
        // MST_BTR
        public string CollectDate { get; set; }
        public string CollectReason { get; set; }
        public string PackModuleCode { get; set; }
        // MST_BTR_TYPE
        public string BatteryTypeName { get; set; }
        public string CarReleaseYear { get; set; }
        // MST_CAR_MAKE
        public string CarMakeName { get; set; }
        // MST_CAR
        public string CarName { get; set; }
        // MST_BTR_MAKE
        public string BatteryMakeName { get; set; }
    }
}
