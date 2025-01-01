using AcquisitionDate.Serializiation;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatabase
{
    IDatableData GetEntry(ulong contentID);
    IDatableData[] GetEntries();

    SerializableUser[] SerializeDatabase();
}
