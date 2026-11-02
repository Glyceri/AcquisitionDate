using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;
using System;
using System.Collections.Generic;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Translation;

namespace AcquisitionDate.Database;

internal class DatableDatabase : IDatabase
{
    private List<IDatableData> _entries = [];

    private readonly IAcquisitionServices Services;
    private readonly IDirtySetter DirtySetter;

    private IDatableData? localPlayerData = null;
    private ulong lastLocalPlayerId = 0;
    private bool lastIsLoaded = false;
    
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
                new DatableList(DirtySetter, user.FishingSpotList, Services.Configuration),
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

    public IDatableData? GetLocalUser()
        => localPlayerData;
    
    public TypedDatabaseEntry[] GetDateLists(AcquirableDateType dateType)
    {
        List<TypedDatabaseEntry> datableLists = [];
        
        foreach (IDatableData data in _entries)
        {
            IDatableList       dataList = data.GetDateList(dateType);
            TypedDatabaseEntry entry    = new TypedDatabaseEntry(data, dataList);
            
            if (data.ContentId == localPlayerData?.ContentId)
            {
                datableLists.Insert(0, entry);
            }
            else
            {
                datableLists.Add(entry);
            }
        }
        
        return datableLists.ToArray();
    }
    
    private IDatableData GetEntry(ulong contentId)
    {
        IDatableData? entry = GetEntryNoCreate(contentId);
        
        if (entry != null)
        {
            return entry;
        }

        IDatableData newEntry = new DatableData(Services, DirtySetter, string.Empty, 0, contentId, null);
        
        _entries.Add(newEntry);
        
        return newEntry;
    }

    public IDatableData[] GetEntries() 
        => _entries.ToArray();

    public IDatableData? GetEntryNoCreate(ulong contentID)
    {
        int entriesCount = _entries.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            IDatableData entry = _entries[i];
            if (entry.ContentId != contentID) continue;
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
    
    public string? GetDateTimeString(uint forID, Func<IDatableData, IDatableList> getListCallback, bool showAlts, IDatableData localUser)
    {
        bool earliestIsLocal = false;

        GetDateValues
        (
            forID, 
            getListCallback, 
            showAlts, 
            localUser,
            out AcquisitionDateTime? earliestTimeStamp,
            out UnlockedDate? earliestTime,
            out IDatableData? earliestData
        );

        if (earliestTime == null || earliestData == null || earliestTimeStamp == null)
        {
            return Services.Configuration.DateStandinString();
        }

        if (earliestData.ContentId == localUser.ContentId)
        {
            earliestIsLocal = true;
        }

        string? dateString = earliestTime?.GetDateTime()?.GetTimeString();
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

    public void GetDateValues(uint forId, Func<IDatableData, IDatableList> getListCallback, bool showAlts, IDatableData localUser,
                       out AcquisitionDateTime? earliestTimeStamp, out UnlockedDate? earliestTime, out IDatableData? earliestData)
    {
        earliestTimeStamp = null;
        earliestTime = null;
        earliestData = null;

        if (Services.Configuration.ShowDatesFromAlts && showAlts)
        {
            foreach (IDatableData entry in _entries)
            {
                IDatableList list = GetList(entry, getListCallback);

                UnlockedDate? date = GetDate(forId, list);
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
        else
        {
            earliestData = localUser;

            IDatableList list = GetList(earliestData, getListCallback);

            UnlockedDate? date = GetDate(forId, list);

            earliestTime = date;
            earliestTimeStamp = date?.GetDateTime();
        }
    }

    IDatableList GetList(IDatableData data, Func<IDatableData, IDatableList> getListCallback) 
        => getListCallback.Invoke(data);

    UnlockedDate? GetDate(uint forID, IDatableList list) 
        => list.GetDate(forID);
    
    public void Update(float deltaTime)
    {
        if (!PluginHandlers.PlayerState.IsLoaded)
        {
            localPlayerData = null;
            
            return;
        }
        
        if (localPlayerData != null)
        {
            return;
        }
        
        ulong playerId = PluginHandlers.PlayerState.ContentId;
        
        lastLocalPlayerId = playerId;
        localPlayerData   = GetEntry(playerId);
        
        localPlayerData.UpdateEntry(playerId, PluginHandlers.PlayerState.CharacterName, (ushort)PluginHandlers.PlayerState.HomeWorld.RowId);
    }
}
