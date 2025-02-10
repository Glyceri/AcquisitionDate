using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;
using AcquistionDate.PetNicknames.TranslatorSystem;
using System;
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
            IDatableData newData = new DatableData
            (
                Services,
                DirtySetter,
                user.Name,
                user.Homeworld,
                user.ContentID,
                user.LodestoneID,
                new DatableList(DirtySetter, user.AchievementList, Services.Configuration),
                new DatableList(DirtySetter, user.QuestList, Services.Configuration),
                new DatableList(DirtySetter, user.MinionList, Services.Configuration),
                new DatableList(DirtySetter, user.MountList, Services.Configuration),
                new DatableList(DirtySetter, user.FacewearList, Services.Configuration),
                new DatableList(DirtySetter, user.OrchestrionList, Services.Configuration),
                new DatableList(DirtySetter, user.ClassLVLList, Services.Configuration),
                new DatableList(DirtySetter, user.CardList, Services.Configuration),
                new DatableList(DirtySetter, user.FashionList, Services.Configuration),
                new DatableList(DirtySetter, user.DutyList, Services.Configuration),
                new DatableList(DirtySetter, user.FishingList, Services.Configuration),
                new DatableList(DirtySetter, user.SightList, Services.Configuration),
                new DatableList(DirtySetter, user.FramersList, Services.Configuration),
                new DatableList(DirtySetter, user.SecretRecipeBookList, Services.Configuration),
                new DatableList(DirtySetter, user.BuddyEquipList, Services.Configuration),
                new DatableList(DirtySetter, user.UnlockLinkList, Services.Configuration),
                new DatableList(DirtySetter, user.FolkloreTomeList, Services.Configuration)
            );

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

    public string? GetDateTimeString(uint forID, Func<IDatableData, IDatableList> getListCallback, IDatableData localUser)
    {
        bool earliestIsLocal = false;

        AcquisitionDateTime? earliestTimeStamp = null;
        UnlockedDate? earliestTime = null;
        IDatableData? earliestData = null;

        if (Services.Configuration.ShowDatesFromAlts == false)
        {
            earliestData = localUser;

            IDatableList list = GetList(earliestData, getListCallback);

            UnlockedDate? date = GetDate(forID, list);

            earliestTime = date;
        }
        else
        {
            foreach (IDatableData entry in _entries)
            {
                IDatableList list = GetList(entry, getListCallback);

                UnlockedDate? date = GetDate(forID, list);
                if (date == null) continue;

                AcquisitionDateTime? dateTime = date.Value.GetDateTime();
                if (dateTime == null) continue;

                earliestTime ??= date;
                earliestData ??= entry;
                earliestTimeStamp ??= dateTime;
                if (earliestTime == null || earliestTimeStamp == null) continue;

                if (earliestTimeStamp.Value.DateTime.Ticks >= dateTime.Value.DateTime.Ticks) continue;

                earliestTime = date;
                earliestData = entry;
                earliestTimeStamp ??= dateTime;
            }
        }

        if (earliestTime == null || earliestData == null || earliestTimeStamp == null)
        {
            return Services.Configuration.DateStandinString();
        }

        if (earliestData.ContentID == localUser.ContentID)
        {
            earliestIsLocal = true;
        }

        string? dateString = earliestTime.Value.GetTimeString();
        if (dateString == null)
        {
            return Services.Configuration.DateStandinString();
        }

        if (earliestTimeStamp.Value.IsLowest)
        {
            dateString = string.Format(Translator.GetLine("DateTimeString.Before"), dateString);
        }

        if (!earliestIsLocal)
        {
            dateString += $" [{earliestData.Name}@{earliestData.HomeworldName}]";
        }

        return dateString;
    }

    IDatableList GetList(IDatableData data, Func<IDatableData, IDatableList> getListCallback) => getListCallback.Invoke(data);

    UnlockedDate? GetDate(uint forID, IDatableList list) => list.GetDate(forID);
}
