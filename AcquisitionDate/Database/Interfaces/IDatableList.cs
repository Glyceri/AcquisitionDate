using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Serializiation;
using System;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatableList
{
    int Length { get; }

    uint GetId(int index);

    UnlockedDate? GetDate(uint id);

    void SetDate(uint id, DateTime? value, AcquiredDateType dateType);
    bool RemoveDate(uint id, AcquiredDateType dateType);
    bool RemoveDate(uint id);

    SerializableList Serialise();
}
