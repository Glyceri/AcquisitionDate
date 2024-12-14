using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Database;

internal class DatableData : IDatableData
{
    public ulong ContentID { get; private set; }
    public string Name { get; private set; } = "";
    public ushort Homeworld { get; private set; }
    public string HomeworldName { get; private set; } = "";

    public ulong? LodestoneID { get; private set; }

    public bool HasSearchedLodestoneID { get; set; } = false;
    public bool IsReady => LodestoneID != null;

    readonly IAcquisitionServices Services;

    public IDatableList AchievementList { get; }
    public IDatableList QuestList { get; }

    public DatableData(IAcquisitionServices services, string name, ushort homeworld, ulong contentID, ulong? lodestoneID, IDatableList achievementList, IDatableList questList) : this(services, name, homeworld, contentID, lodestoneID)
    {
        AchievementList = achievementList;
        QuestList = questList;
    }

    public DatableData(IAcquisitionServices services, string name, ushort homeworld, ulong contentID, ulong? lodestoneID)
    {
        Services = services;

        Name = name;
        Homeworld = homeworld;
        ContentID = contentID;
        LodestoneID = lodestoneID;

        AchievementList = new DatableList();
        QuestList = new DatableList();
    }

    public void UpdateEntry(IDatableUser datableUser)
    {
        SetContentID(datableUser.ContentID);
        SetName(datableUser.Name);
        SetHomeworld(datableUser.Homeworld);
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
    }
}
