namespace GbtpLib.Mssql.Domain
{
    // Consolidated DTO models for Mssql Domain layer

    // Key and update DTOs for warehouse slots
    public class WarehouseSlotKeyDto
    {
        public string SiteCode { get; set; } = string.Empty;
        public string FactoryCode { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public string Row { get; set; } = string.Empty;
        public string Col { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }

    public sealed class WarehouseSlotUpdateDto : WarehouseSlotKeyDto
    {
        public string LabelId { get; set; } = string.Empty;
        public string LoadGrade { get; set; } = string.Empty;
    }

    // Label information
    public sealed class LabelInfoDto
    {
        public string Site { get; set; } = string.Empty;
        public string CollectionDate { get; set; } = string.Empty;
        public string ColtDat { get; set; } = string.Empty;
        public string CarMakeNm { get; set; } = string.Empty;
        public string CarNm { get; set; } = string.Empty;
        public string CarRelsYear { get; set; } = string.Empty;
        public string BtrMakeNm { get; set; } = string.Empty;
        public string BtrTypeNm { get; set; } = string.Empty;
    }

    // Simple code-name pairs
    public sealed class CodeNameDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public sealed class WarehouseCodeNameDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    // Slot layout
    public sealed class WarehouseSlotLayoutDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LabelId { get; set; } = string.Empty;
    }

    // Label creation metadata
    public sealed class LabelCreationInfoDto
    {
        public string CarMakeCode { get; set; } = string.Empty;
        public string CarMakeName { get; set; } = string.Empty;
        public string CarCode { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string BatteryMakeCode { get; set; } = string.Empty;
        public string BatteryMakeName { get; set; } = string.Empty;
        public string CarReleaseYear { get; set; } = string.Empty;
        public int BatteryTypeNo { get; set; }
        public string BatteryTypeSelectCode { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
    }

    // Slot info for UI
    public sealed class SlotInfoDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LabelId { get; set; } = string.Empty;
        public string InspectGrade { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string CollectDate { get; set; } = string.Empty;
        public string CollectReason { get; set; } = string.Empty;
        public string PackModuleCode { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
        public string CarReleaseYear { get; set; } = string.Empty;
        public string CarMakeName { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string BatteryMakeName { get; set; } = string.Empty;
        public string StoreDiv { get; set; } = string.Empty;
        public string PrintYn { get; set; } = string.Empty;
        public double VoltMaxValue { get; set; }
        public double VoltMinValue { get; set; }
        public double AcirMaxValue { get; set; }
        public double InsulationMinValue { get; set; }
    }

    // Defect battery info
    public sealed class DefectBatteryDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LabelId { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string CollectDate { get; set; } = string.Empty;
        public string CollectReason { get; set; } = string.Empty;
        public string DigInspFlag { get; set; } = string.Empty;
        public string PackModuleCode { get; set; } = string.Empty;
        public string BatteryTypeName { get; set; } = string.Empty;
        public string CarReleaseYear { get; set; } = string.Empty;
        public string CarMakeName { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public string BatteryMakeName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
    }

    // Grade-class battery search result
    public sealed class GradeClassBatteryDbDto
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LblId { get; set; } = string.Empty;
        public string InspGrd { get; set; } = string.Empty;
        public string SiteNm { get; set; } = string.Empty;
        public string ColtDat { get; set; } = string.Empty;
        public string ColtResn { get; set; } = string.Empty;
        public string PackMdleCd { get; set; } = string.Empty;
        public string BtrTypeNm { get; set; } = string.Empty;
        public string CarRelsYear { get; set; } = string.Empty;
        public string CarMakeNm { get; set; } = string.Empty;
        public string CarNm { get; set; } = string.Empty;
        public string BtrMakeNm { get; set; } = string.Empty;
    }

    public sealed class BatteryTypeYearDto
    {
        public string BatteryTypeName { get; set; } = string.Empty; // MST_BTR_TYPE.BTR_TYPE_NM
        public string CarReleaseYear { get; set; } = string.Empty;   // MST_BTR_TYPE.CAR_RELS_YEAR
    }
}
