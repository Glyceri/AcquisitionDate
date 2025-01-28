namespace AcquisitionDate.Parser.Structs;

internal struct LodestoneParseData
{
    public readonly string UserName;
    public readonly uint Homeworld;
    public readonly uint LodestoneID;

    public LodestoneParseData(string userName, uint homeworld, uint lodestoneID) 
    { 
        UserName = userName; 
        Homeworld = homeworld; 
        LodestoneID = lodestoneID; 
    }
}
