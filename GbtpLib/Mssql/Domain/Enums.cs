using System;

namespace GbtpLib.Mssql.Domain
{
    // Enums mirrored from Reference layer for consistency
    public enum IfCmd
    {
        AA0, AA1, AA2, AA3, AA4, AA5, HH3, HH4,
        EE1, EE2, EE3, EE4, EE5, EE6, EE7, EE8,
        GG1,
        UNUSED,
    }

    public enum IfFlag
    {
        C, // Created / Pending
        Y, // Completed / Acknowledged
    }

    public enum YnFlag
    {
        Y,
        N,
    }

    public enum InspectionResult
    {
        None,
        Pass,
        Fail,
    }

    public enum RequestStatus
    {
        Waiting,
        InProgress,
        Finished,
    }
}
