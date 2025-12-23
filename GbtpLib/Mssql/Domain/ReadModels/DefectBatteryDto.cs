namespace GbtpLib.Mssql.Domain.ReadModels
{
    public class DefectBatteryDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LabelId { get; set; }
        public string SiteName { get; set; }
        public string CollectDate { get; set; }
        public string CollectReason { get; set; }
        public string DigInspFlag { get; set; }
        public string PackModuleCode { get; set; }
        public string BatteryTypeName { get; set; }
        public string CarReleaseYear { get; set; }
        public string CarMakeName { get; set; }
        public string CarName { get; set; }
        public string BatteryMakeName { get; set; }
        public string Grade { get; set; }
    }
}
