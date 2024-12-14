using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AcquisitionDate.DatableUsers;

internal unsafe class UserList : IUserList
{
    public IDatableUser? ActiveUser { get; private set; }

    readonly IDatabase Database;

    public UserList(IDatabase database)
    {
        Database = database;
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
    }

    public void Delete(BattleChara* bChara)
    {
        if (!ActiveUserEquals(bChara)) return;

        ActiveUser?.Dispose();
        ActiveUser = null;
    }

    bool ActiveUserEquals(BattleChara* bChara)
    {
        if (ActiveUser == null) return false;

        return ActiveUser.Self == bChara;
    }
}
