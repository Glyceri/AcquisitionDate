using AcquisitionDate.Database.Enums;
using AcquisitionDate.Serializiation;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatableData
{
    string Name                   { get; }
    ushort Homeworld              { get; }
    string HomeworldName          { get; }
    ulong  ContentId              { get; }
    ulong? LodestoneId            { get; }
    bool   IsLocalPlayer          { get; }
    bool   HasSearchedLodestoneId { get; }
    bool   IsReady                { get; }

    IDatableList GetDateList(AcquirableDateType acquireableDate);

    void UpdateEntry(ulong contentId, string name, ushort homeworld);
    void SetLodestoneId(ulong lodestoneId);

    SerializableUser SerializeEntry();
}
