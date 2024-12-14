using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Serializiation;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatableData
{
    string Name { get; }
    ushort Homeworld { get; }
    string HomeworldName { get; }
    ulong ContentID { get; }
    ulong? LodestoneID { get; }

    // Should NOT be serialized
    bool HasSearchedLodestoneID { get; set; }
    bool IsReady { get; }

    IDatableList AchievementList { get; }
    IDatableList QuestList { get; }

    void UpdateEntry(IDatableUser datableUser);
    void SetLodestoneID(ulong lodestoneID);

    SerializableUser SerializeEntry();
}
