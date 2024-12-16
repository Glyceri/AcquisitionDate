using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AcquisitionDate.DatableUsers;

internal unsafe class UserList : IUserList
{
    ulong lastContentID = 0;
    public IDatableUser? ActiveUser { get; private set; }

    readonly IDatabase Database;
    readonly IDirtySetter DirtySetter;

    public UserList(IDatabase database, IDirtySetter dirtySetter)
    {
        Database = database;
        DirtySetter = dirtySetter;
    }

    public void Create(BattleChara* bChara)
    {
        if (ActiveUserEquals(bChara))
        {
            PluginHandlers.PluginLog.Error("You shouldn't be creating a DatableUser when one with the same identifier is already active");
            return;
        }

        ActiveUser?.Dispose();
        ActiveUser = null;

        ActiveUser = new DatableUser(Database, bChara);
        PluginHandlers.PluginLog.Verbose($"Created new user with the Name: {ActiveUser.Name} and the ContentID: {ActiveUser.ContentID}");

        if (lastContentID == ActiveUser.ContentID) return;
        lastContentID = ActiveUser.ContentID;

        DirtySetter?.NotifyDirtyUser();
    }

    public void Delete(BattleChara* bChara)
    {
        if (!ActiveUserEquals(bChara)) return;

        if (ActiveUser == null) return;
        
        PluginHandlers.PluginLog.Verbose($"Destroyed the user with the Name: {ActiveUser.Name} and the ContentID: {ActiveUser.ContentID}");

        ActiveUser?.Dispose();
        ActiveUser = null;
    }

    bool ActiveUserEquals(BattleChara* bChara)
    {
        if (ActiveUser == null) return false;

        return ActiveUser.Self == bChara;
    }
}
