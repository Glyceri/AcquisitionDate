namespace AcquisitionDate.Parser.Structs;

internal struct LodestoneParseData
{
    public readonly string UserName;
    public readonly uint Homeworld;
    public readonly uint LodestoneID;
    public readonly string IconURL;

    public LodestoneParseData(string userName, uint homeworld, uint lodestoneID, string iconURL) 
    { 
        UserName = userName; 
        Homeworld = homeworld; 
        LodestoneID = lodestoneID; 
        IconURL = iconURL;
    }
}
