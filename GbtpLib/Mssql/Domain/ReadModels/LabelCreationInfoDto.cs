namespace GbtpLib.Mssql.Domain.ReadModels
{
    public class LabelCreationInfoDto
    {
        public string CarMakeCode { get; set; }
        public string CarMakeName { get; set; }
        public string CarCode { get; set; }
        public string CarName { get; set; }
        public string BatteryMakeCode { get; set; }
        public string BatteryMakeName { get; set; }
        public string CarReleaseYear { get; set; }
        public int BatteryTypeNo { get; set; }
        public string BatteryTypeSelectCode { get; set; }
        public string BatteryTypeName { get; set; }
    }
}
