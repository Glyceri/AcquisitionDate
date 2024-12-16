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
using AcquisitionDate.Serializiation;
using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.Acquisition;
using AcquisitionDate.DirtySystem;

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
    readonly IAcquirerHandler AcquirerHandler;

    readonly DirtyHandler DirtyHandler;
    readonly SaveHandler SaveHandler;

    public AcquisitionDatePlugin(IDalamudPluginInterface pluginInterface)
    {
        PluginHandlers.Start(ref pluginInterface, this);
        Services = new AcquisitionSevices();

        DirtyHandler = new DirtyHandler();
        SaveHandler = new SaveHandler(DirtyHandler, Services.Configuration);

        Database = new DatableDatabase(Services, DirtyHandler);
        UserList = new UserList(Database, DirtyHandler);

        LodestoneNetworker = new LodestoneNetworker();
        HookHandler = new HookHandler(Services, UserList, DirtyHandler);
        UpdateHandler = new UpdateHandler(LodestoneNetworker, UserList, Services, SaveHandler, HookHandler.UnlocksHook);
        AcquirerHandler = new AcquirerHandler(Services, LodestoneNetworker);
        WindowHandler = new WindowHandler(Services, pluginInterface, UserList, Database);

        Services.Configuration.Initialise(Database);
    }

    public void Dispose()
    {
        HookHandler?.Dispose();
        WindowHandler?.Dispose();
        UpdateHandler?.Dispose();
        LodestoneNetworker?.Dispose();
        SaveHandler?.Dispose();
        AcquirerHandler?.Dispose();
    }
}
