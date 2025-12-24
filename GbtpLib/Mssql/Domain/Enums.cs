using System;

namespace GbtpLib.Mssql.Domain
{
    // Enums mirrored from Reference layer for consistency
    public enum EIfCmd
    {
        // 입고(Incoming)
        AA0, AA1, AA2, AA3, AA4, AA5,
        // 활성화/진단(Activation/Diagnosis)
        BB0, BB1, BB2, BB3, BB4, BB5, BB6, BB7, BB8, BB9, BB10, BB11,
        // 활성화 재입고(Return to warehouse)
        CC1, CC2,
        // 진단(Diagnosis to rail/load)
        DD1, DD2,
        // 출고(Outcome)
        EE1, EE2, EE3, EE4, EE5, EE6, EE7, EE8,
        // 불량적재(Defect)
        FF1, FF2,
        // 비상정지(Emergency)
        HH3, HH4, HH5,
        // 재입고(Defect re-incoming)
        GG1,
        UNUSED,
    }

    public enum EIfFlag
    {
        C, // Created / Pending
        Y, // Completed / Acknowledged
    }

    public enum EYnFlag
    {
        Y,
        N,
    }

    public enum EInspectionResult
    {
        None,
        Pass,
        Fail,
    }

    public enum ERequestStatus
    {
        Waiting,
        InProgress,
        Finished,
    }
}
