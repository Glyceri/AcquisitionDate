using AcquisitionDate.Serializiation;

namespace AcquisitionDate.Database.Interfaces;

internal interface IDatabase
{
    IDatableData GetEntry(ulong contentID);

    SerializableUser[] SerializeDatabase();
}
