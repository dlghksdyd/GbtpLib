namespace GbtpLib.Mssql.Domain
{
    public class CodeNameDto { public string Code { get; set; } public string Name { get; set; } }

    public class WarehouseCodeNameDto { public string Code { get; set; } public string Name { get; set; } }

    public class WarehouseSlotLayoutDto { public int Row { get; set; } public int Col { get; set; } public int Lvl { get; set; } public string LabelId { get; set; } }

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

    public class SlotInfoDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LabelId { get; set; }
        public string InspectGrade { get; set; }
        public string SiteName { get; set; }
        public string CollectDate { get; set; }
        public string CollectReason { get; set; }
        public string PackModuleCode { get; set; }
        public string BatteryTypeName { get; set; }
        public string CarReleaseYear { get; set; }
        public string CarMakeName { get; set; }
        public string CarName { get; set; }
        public string BatteryMakeName { get; set; }
        // Added fields for UI/logic
        public string StoreDiv { get; set; }
        public string PrintYn { get; set; }
        public double VoltMaxValue { get; set; }
        public double VoltMinValue { get; set; }
        public double AcirMaxValue { get; set; }
        public double InsulationMinValue { get; set; }
    }

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
