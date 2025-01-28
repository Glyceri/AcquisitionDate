using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Database;

internal class DatableDatabase : IDatabase
{
    List<IDatableData> _entries = new List<IDatableData>();

    readonly IAcquisitionServices Services;
    readonly IDirtySetter DirtySetter;

    public DatableDatabase(IAcquisitionServices services, IDirtySetter dirtySetter)
    {
        Services = services;
        DirtySetter = dirtySetter;

        Setup();
    }

    void Setup()
    {
        SerializableUser[]? users = Services.Configuration.SerializableUsers;
        if (users == null) return;

        foreach (SerializableUser user in users)
        {
            IDatableData newData = new DatableData(Services, DirtySetter,
                user.Name,
                user.Homeworld,
                user.ContentID,
                user.LodestoneID,
                new DatableList(DirtySetter, user.AchievementList),
                new DatableList(DirtySetter, user.QuestList), 
                new DatableList(DirtySetter, user.MinionList),
                new DatableList(DirtySetter, user.MountList),
                new DatableList(DirtySetter, user.FacewearList),
                new DatableList(DirtySetter, user.OrchestrionList),
                new DatableList(DirtySetter, user.ClassLVLList),
                new DatableList(DirtySetter, user.CardList),
                new DatableList(DirtySetter, user.FashionList),
                new DatableList(DirtySetter, user.DutyList),
                new DatableList(DirtySetter, user.FishingList),
                new DatableList(DirtySetter, user.SightList),
                new DatableList(DirtySetter, user.FramersList),
                new DatableList(DirtySetter, user.SecretRecipeBookList),
                new DatableList(DirtySetter, user.BuddyEquipList),
                new DatableList(DirtySetter, user.UnlockLinkList),
                new DatableList(DirtySetter, user.FolkloreTomeList),
                new DatableList(DirtySetter, user.EmoteList));

            _entries.Add(newData);
        }
    }

    public IDatableData GetEntry(ulong contentID)
    {
        IDatableData? entry = GetEntryNoCreate(contentID);
        if (entry != null) return entry;

        IDatableData newEntry = new DatableData(Services, DirtySetter, string.Empty, 0, contentID, null);
        _entries.Add(newEntry);
        return newEntry;
    }

    public IDatableData[] GetEntries() => _entries.ToArray();

    public IDatableData? GetEntryNoCreate(ulong contentID)
    {
        int entriesCount = _entries.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            IDatableData entry = _entries[i];
            if (entry.ContentID != contentID) continue;
            return _entries[i];
        }

        return null;
    }

    public SerializableUser[] SerializeDatabase()
    {
        List<SerializableUser> users = new List<SerializableUser>();

        int entryCount = _entries.Count;
        for (int i = 0; i < entryCount; i++)
        {
            IDatableData data = _entries[i];
            users.Add(data.SerializeEntry());
        }

        return users.ToArray();
    }

    public void SetDirty()
    {
        DirtySetter.NotifyDirtyDatabase();
    }
}
