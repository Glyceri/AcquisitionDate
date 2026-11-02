using AcquisitionDate.Database.Interfaces;

namespace AcquisitionDate.Database.Structs;

internal readonly struct TypedDatabaseEntry(IDatableData data, IDatableList list)
{
    public readonly IDatableData Data = data;
    public readonly IDatableList List = list;
}