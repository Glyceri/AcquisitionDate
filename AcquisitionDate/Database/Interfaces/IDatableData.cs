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
    IDatableList MinionList { get; }
    IDatableList MountList { get; }

    IDatableList FacewearList { get; }
    IDatableList OrchestrionList { get; }
    IDatableList ClassLVLList { get; }
    IDatableList CardList { get; }
    IDatableList FashionList { get; }
    IDatableList DutyList { get; }
    IDatableList FishingList { get; }
    IDatableList SightList { get; }

    IDatableList FramersList { get; }
    IDatableList SecretRecipeBookList { get; }
    IDatableList BuddyEquipList { get; }
    IDatableList UnlockLinkList { get; }
    IDatableList FolkloreTomeList { get; }

    void UpdateEntry(IDatableUser datableUser);
    void SetLodestoneID(ulong lodestoneID);

    SerializableUser SerializeEntry();
}
