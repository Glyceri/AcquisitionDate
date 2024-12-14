using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AcquisitionDate.DatableUsers;

internal unsafe class DatableUser : IDatableUser
{
    public BattleChara* Self { get; }

    public string Name { get; } = string.Empty;
    public ushort Homeworld { get; }
    public ulong ContentID { get; }
    public ulong? LodestoneID => Data.LodestoneID;

    public bool IsReady => Data.IsReady;

    public IDatableData Data { get; }

    public DatableUser(IDatabase database, BattleChara* bChara)
    {
        Self = bChara;

        Name = Self->NameString;
        ContentID = Self->ContentId;
        Homeworld = Self->HomeWorld;

        Data = database.GetEntry(ContentID);
        Data.UpdateEntry(this);
    }

    public void Dispose()
    {
        
    }
}
