using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Serializiation;
using System;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatableList
{
    int Length { get; }

    uint GetID(int index);

    UnlockedDate? GetDate(uint ID);

    void SetDate(uint ID, DateTime? value, AcquiredDateType dateType);
    bool RemoveDate(uint ID, AcquiredDateType dateType);
    bool RemoveDate(uint ID);

    SerializableList Serialise();
}
