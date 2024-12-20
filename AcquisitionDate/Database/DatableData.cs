using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Database;

internal class DatableData : IDatableData
{
    public ulong ContentID { get; private set; }
    public string Name { get; private set; } = "";
    public ushort Homeworld { get; private set; }
    public string HomeworldName { get; private set; } = "";

    public ulong? LodestoneID { get; private set; } = null;

    public bool HasSearchedLodestoneID { get; set; } = false;
    public bool IsReady => LodestoneID != null;

    public IDatableList AchievementList { get; }
    public IDatableList QuestList { get; }
    public IDatableList MinionList { get; }
    public IDatableList MountList { get; }
    public IDatableList FacewearList { get; }
    public IDatableList OrchestrionList { get; }
    public IDatableList ClassLVLList { get; }
    public IDatableList CardList { get; }
    public IDatableList FashionList { get; }
    public IDatableList DutyList { get; }
    public IDatableList FishingList { get; }
    public IDatableList SightList { get; }
    public IDatableList FramersList { get; }
    public IDatableList SecretRecipeBookList { get; }
    public IDatableList BuddyEquipList { get; }
    public IDatableList UnlockLinkList { get; }
    public IDatableList FolkloreTomeList { get; }

    readonly IAcquisitionServices Services;
    readonly IDirtySetter DirtySetter;

    public DatableData(
        IAcquisitionServices services,
        IDirtySetter dirtySetter,
        string name,
        ushort homeworld,
        ulong contentID,
        ulong? lodestoneID,
        IDatableList achievementList,
        IDatableList questList,
        IDatableList minionList,
        IDatableList mountList,
        IDatableList facewearList,
        IDatableList orchestrionList,
        IDatableList classLVLList,
        IDatableList cardList,
        IDatableList fashionList,
        IDatableList dutyList,
        IDatableList fishingList,
        IDatableList sightList,
        IDatableList framerList,
        IDatableList secretRecipeList,
        IDatableList buddyEquipList,
        IDatableList unlockLinkList,
        IDatableList folkloreList)
    {
        Services = services;
        DirtySetter = dirtySetter;

        SetName(name);
        SetHomeworld(homeworld);
        SetContentID(contentID);
        if (lodestoneID != null)
        {
            SetLodestoneID(lodestoneID.Value);
        }

        AchievementList = achievementList;
        QuestList = questList;
        MinionList = minionList;
        MountList = mountList;
        FacewearList = facewearList;
        OrchestrionList = orchestrionList;
        ClassLVLList = classLVLList;
        CardList = cardList;
        FashionList = fashionList;
        DutyList = dutyList;
        FishingList = fishingList;
        SightList = sightList;
        FramersList = framerList;
        SecretRecipeBookList = secretRecipeList;
        BuddyEquipList = buddyEquipList;
        UnlockLinkList = unlockLinkList;
        FolkloreTomeList = folkloreList;
    }

    public DatableData(IAcquisitionServices services, IDirtySetter dirtySetter, string name, ushort homeworld, ulong contentID, ulong? lodestoneID)
    {
        Services = services;
        DirtySetter = dirtySetter;

        SetName(name);
        SetHomeworld(homeworld);
        SetContentID(contentID);
        if (lodestoneID != null)
        {
            SetLodestoneID(lodestoneID.Value);
        }

        AchievementList = new DatableList(DirtySetter);
        QuestList = new DatableList(DirtySetter);
        MinionList = new DatableList(DirtySetter);
        MountList = new DatableList(DirtySetter);
        FacewearList = new DatableList(DirtySetter);
        OrchestrionList = new DatableList(DirtySetter);
        ClassLVLList = new DatableList(DirtySetter);
        CardList = new DatableList(DirtySetter);
        FashionList = new DatableList(DirtySetter);
        DutyList = new DatableList(DirtySetter);
        FishingList = new DatableList(DirtySetter);
        SightList = new DatableList(DirtySetter);
        FramersList = new DatableList(DirtySetter);
        SecretRecipeBookList = new DatableList(DirtySetter);
        BuddyEquipList = new DatableList(DirtySetter);
        UnlockLinkList = new DatableList(DirtySetter);
        FolkloreTomeList = new DatableList(DirtySetter);
    }

    public void UpdateEntry(IDatableUser datableUser)
    {
        SetContentID(datableUser.ContentID);
        SetName(datableUser.Name);
        SetHomeworld(datableUser.Homeworld);

        MarkDirty();
    }

    void SetHomeworld(ushort homeworld)
    {
        Homeworld = homeworld;
        HomeworldName = Services.Sheets.GetWorldName(Homeworld) ?? "...";
    }

    void SetName(string name)
    {
        Name = name;
    }

    void SetContentID(ulong contentID)
    {
        ContentID = contentID;
    }

    public void SetLodestoneID(ulong lodestoneID)
    {
        LodestoneID = lodestoneID;
        HasSearchedLodestoneID = true;

        MarkDirty();
    }

    void MarkDirty()
    {
        DirtySetter.NotifyDirtyDatabase();
    }

    public SerializableUser SerializeEntry() => new SerializableUser(this);
}
