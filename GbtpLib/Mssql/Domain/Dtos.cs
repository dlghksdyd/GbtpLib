using System;

namespace GbtpLib.Mssql.Domain
{
    // Simple DTOs used by application use-cases

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

        public RequestStatus Status { get; set; }
    }

    public class StoredProcedureParamDto
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
