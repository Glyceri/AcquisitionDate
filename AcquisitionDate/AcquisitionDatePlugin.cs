using Dalamud.Plugin;
using AcquisitionDate.Windows;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking;
using AcquisitionDate.Updating.Interfaces;
using AcquisitionDate.Updating;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database;
using AcquisitionDate.DatableUsers;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Hooking;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Services;

namespace AcquisitionDate;

public sealed class AcquisitionDatePlugin : IDalamudPlugin
{
    internal WindowHandler WindowHandler { get; private set; }

    readonly IAcquisitionServices Services;
    readonly IUpdateHandler UpdateHandler;
    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IHookHandler HookHandler;

    readonly IDatabase Database;
    readonly IUserList UserList;

    public AcquisitionDatePlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        Services = new AcquisitionSevices();

        Database = new DatableDatabase(Services);
        UserList = new UserList(Database);

        LodestoneNetworker = new LodestoneNetworker();
        UpdateHandler = new UpdateHandler(LodestoneNetworker, UserList, Services);
        HookHandler = new HookHandler(UserList);

        WindowHandler = new WindowHandler(pluginInterface, UserList, Database);
    }

    public void Dispose()
    {
        HookHandler?.Dispose();
        WindowHandler?.Dispose();
        UpdateHandler?.Dispose();
        LodestoneNetworker?.Dispose();
    }
}
