using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Database;

internal class DatableData : IDatableData
{
    readonly IDatableList[] AllDatableData = new IDatableList[(int)AcquirableDateType.COUNT];

    public ulong ContentID { get; private set; }
    public string Name { get; private set; } = "";
    public ushort Homeworld { get; private set; }
    public string HomeworldName { get; private set; } = "";

    public ulong? LodestoneID { get; private set; } = null;

    public bool HasSearchedLodestoneID { get; set; } = false;
    public bool IsReady => LodestoneID != null;

    readonly IAcquisitionServices Services;
    readonly IDirtySetter DirtySetter;

    public DatableData(
        IAcquisitionServices services,
        IDirtySetter dirtySetter,
        string name,
        ushort homeworld,
        ulong contentID,
        ulong? lodestoneID,
        IDatableList? achievementList       = null,
        IDatableList? questList             = null,
        IDatableList? minionList            = null,
        IDatableList? mountList             = null,
        IDatableList? facewearList          = null,
        IDatableList? orchestrionList       = null,
        IDatableList? classLVLList          = null,
        IDatableList? cardList              = null,
        IDatableList? fashionList           = null,
        IDatableList? dutyList              = null,
        IDatableList? fishingList           = null,
        IDatableList? sightList             = null,
        IDatableList? framerList            = null,
        IDatableList? secretRecipeList      = null,
        IDatableList? buddyEquipList        = null,
        IDatableList? unlockLinkList        = null,
        IDatableList? folkloreList          = null)
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

        AllDatableData[(int)AcquirableDateType.Achievement]         = achievementList       ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Quest]               = questList             ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Minion]              = minionList            ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Mount]               = mountList             ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Facewear]            = facewearList          ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Orchestrion]         = orchestrionList       ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.ClassLVL]            = classLVLList          ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Card]                = cardList              ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Fashion]             = fashionList           ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Duty]                = dutyList              ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Fishing]             = fishingList           ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Incognita]           = sightList             ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.Framers]             = framerList            ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.SecretRecipeBook]    = secretRecipeList      ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.BuddyEquip]          = buddyEquipList        ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.UnlockLink]          = unlockLinkList        ?? new DatableList(DirtySetter, Services.Configuration);
        AllDatableData[(int)AcquirableDateType.FolkloreTome]        = folkloreList          ?? new DatableList(DirtySetter, Services.Configuration);
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

    // The plugin should straight up crash IMO if flag doesnt get found
    public IDatableList GetDate(AcquirableDateType flag)
    {
        return AllDatableData[(int)flag];
    }
}
