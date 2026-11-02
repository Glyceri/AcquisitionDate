using AcquisitionDate.Serializiation;
using System;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatabase : IUpdatable
{
    IDatableData[] GetEntries();

    SerializableUser[] SerializeDatabase();
    IDatableData? GetLocalUser();
    
    //bool GetDateTime(AcquirableDateType dataType, uint index, out 
    
    TypedDatabaseEntry[] GetDateLists(AcquirableDateType dateType);
    
    string? GetDateTimeString(uint forId, Func<IDatableData, IDatableList> getListCallback, bool showAlts, IDatableData localUser);
}
