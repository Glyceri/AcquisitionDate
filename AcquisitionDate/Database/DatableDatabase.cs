using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Database;

internal class DatableDatabase : IDatabase
{
    List<IDatableData> _entries = new List<IDatableData>();

    readonly IAcquisitionServices Services;

    public DatableDatabase(IAcquisitionServices services)
    {
        Services = services;
    }

    public IDatableData GetEntry(ulong contentID)
    {
        IDatableData? entry = GetEntryNoCreate(contentID);
        if (entry != null) return entry;

        IDatableData newEntry = new DatableData(Services, string.Empty, 0, contentID, null);
        _entries.Add(newEntry);
        return newEntry;
    }

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
}
