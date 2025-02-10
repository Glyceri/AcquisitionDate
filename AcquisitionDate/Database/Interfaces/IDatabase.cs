using AcquisitionDate.Serializiation;
using System;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatabase
{
    IDatableData GetEntry(ulong contentID);
    IDatableData[] GetEntries();

    SerializableUser[] SerializeDatabase();

    string? GetDateTimeString(uint forID, Func<IDatableData, IDatableList> getListCallback, IDatableData localUser);
}
