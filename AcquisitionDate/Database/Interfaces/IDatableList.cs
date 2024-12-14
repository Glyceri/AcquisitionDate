using AcquisitionDate.Database.Enums;
using System;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatableList
{
    int Length { get; }

    // Get's the prefferred most accurate date!
    DateTime? GetDate(uint ID);
    DateTime? GetDate(uint ID, AcquiredDateType dateType);
    void SetDate(uint ID, DateTime? value, AcquiredDateType dateType);
    bool RemoveDate(uint ID, AcquiredDateType dateType);
    bool RemoveDate(uint ID);
}
