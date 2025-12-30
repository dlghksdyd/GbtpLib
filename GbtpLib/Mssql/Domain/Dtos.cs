using System;

namespace GbtpLib.Mssql.Domain
{
    // Simple DTOs used by application use-cases (PascalCase properties)

    public class WarehouseSlotKeyDto
    {
        public string SiteCode { get; set; }
        public string FactoryCode { get; set; }
        public string WarehouseCode { get; set; }
        public string Row { get; set; }
        public string Col { get; set; }
        public string Level { get; set; }
    }

    public class WarehouseSlotUpdateDto : WarehouseSlotKeyDto
    {
        public string LabelId { get; set; }
        public string LoadGrade { get; set; }
    }

    public class LabelInfoDto
    {
        public string LabelId { get; set; }
        public string CarManufacturer { get; set; }
        public string CarModel { get; set; }
        public string BatteryManufacturer { get; set; }
        public string ReleaseYear { get; set; }
        public string BatteryType { get; set; }
        public string PackOrModule { get; set; }
        public string Site { get; set; }
        public string CollectionDate { get; set; }
        public string CollectionReason { get; set; }
        public string Grade { get; set; }
    }

    public class TransferRequestDto
    {
        public string Label { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Level { get; set; }

        public int LoadingRow { get; set; }
        public int LoadingColumn { get; set; }
        public int LoadingLevel { get; set; }

        public ERequestStatus Status { get; set; }
    }

    public class StoredProcedureParamDto
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    // New: detailed inspection info for income/outcome view models
    public class InoutInspectionInfoDto
    {
        public string LabelId { get; set; }
        public int InspectSeqInt { get; set; }
        public string InspectValueJson { get; set; }
        public int InspectResultEnum { get; set; }
        public string InspectStart { get; set; }
        public string InspectEnd { get; set; }
        public string Note { get; set; }
        public string RegId { get; set; }

        // From MST_BTR
        public string CollectDate { get; set; }

        // From MST_BTR_TYPE and related
        public string BatteryTypeName { get; set; }
        public string CarReleaseYear { get; set; }
        public string CarMakeName { get; set; }
        public string CarName { get; set; }
        public string BatteryMakeName { get; set; }
    }

    // New: save DTO for in/out inspection
    public class InoutInspectionSaveDto
    {
        public string SiteCode { get; set; }
        public string FactoryCode { get; set; }
        public string ProcessCode { get; set; }
        public string MachineCode { get; set; }
        public string InspKindGroupCode { get; set; }
        public string InspKindCode { get; set; }
        public string LabelId { get; set; }
        public int InspectSeq { get; set; }
        public string InspectValueJson { get; set; }
        public int InspectResultEnum { get; set; }
        public DateTime InspectStart { get; set; }
        public DateTime InspectEnd { get; set; }
        public string Note { get; set; }
        public string RegId { get; set; }
        public DateTime RegDateTime { get; set; }
    }

    // Moved from IncomeSystem: local snapshot DTOs for warehouse/label info used by UI
    public class WarehouseInfo
    {
        public string Row { get; set; }
        public string Col { get; set; }
        public string Lvl { get; set; }
        public string LblId { get; set; }
        public string StoreDiv { get; set; }
        public string WhCd { get; set; }
    }

    public class LabelInfo
    {
        // INV_WAREHOUSE
        public int Row { get; set; }
        public int Col { get; set; }
        public int Lvl { get; set; }
        public string LblId { get; set; }
        public string LoadGrd { get; set; }
        public string Sts { get; set; }
        // MST_SITE
        public string SiteNm { get; set; }
        // MST_BTR
        public string BtrTypeNo { get; set; }
        public string ColtDat { get; set; }
        public string ColtResn { get; set; }
        public EYnFlag PrtYn { get; set; }
        public string BtrSts { get; set; }
        public int Mile { get; set; }
        public string Note { get; set; }
        public EYnFlag StoInspFlag { get; set; }
        // MST_BTR_TYPE
        public string BtrTypeNm { get; set; }
        public string CarMakeCd { get; set; }
        public string CarCd { get; set; }
        public string BtrMakeCd { get; set; }
        public string CarRelsYear { get; set; }
        public string PackMdleCd { get; set; }
        public double VoltMaxValue { get; set; }
        public double VoltMinValue { get; set; }
        public double AcirMaxValue { get; set; }
        public double InsulMinValue { get; set; }
        // MST_CAR_MAKE
        public string CarMakeNm { get; set; }
        // MST_CAR
        public string CarNm { get; set; }
        // MST_BTR_MAKE
        public string BtrMakeNm { get; set; }
    }
}
