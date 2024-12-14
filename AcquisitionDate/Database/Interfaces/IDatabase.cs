namespace AcquisitionDate.Database.Interfaces;

internal interface IDatabase
{
    IDatableData GetEntry(ulong contentID);
}
