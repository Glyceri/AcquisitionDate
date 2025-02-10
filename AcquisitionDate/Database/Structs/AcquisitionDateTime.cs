using System;

namespace AcquisitionDate.Database.Structs;

internal readonly struct AcquisitionDateTime
{
    public readonly DateTime DateTime;
    public readonly bool IsLowest;

    public AcquisitionDateTime(DateTime dateTime, bool isLowest)
    {
        DateTime = dateTime;
        IsLowest = isLowest;
    }
}
