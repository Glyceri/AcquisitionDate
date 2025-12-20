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
using AcquistionDate.PetNicknames.TranslatorSystem;
using AcquisitionDate.AcquisitionDate.Commands;
using System.Reflection;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Parser;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.ImageDatabase;
using System;

namespace AcquisitionDate;

public sealed class AcquisitionDatePlugin : IDalamudPlugin
{
    public readonly string Version;

    internal WindowHandler WindowHandler { get; private set; }

    readonly IImageDatabase ImageDatabase;
    readonly IImageReader ImageReader;
    readonly IAcquisitionParser AcquistionParser;
    readonly IAcquisitionServices Services;
    readonly IUpdateHandler UpdateHandler;
    readonly INetworkClient NetworkClient;
    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IHookHandler HookHandler;

    readonly IDatabase Database;
    readonly IUserList UserList;
    readonly IAcquirerHandler AcquirerHandler;

    readonly CommandHandler CommandHandler;
    readonly DirtyHandler DirtyHandler;
    readonly SaveHandler SaveHandler;

    public AcquisitionDatePlugin(IDalamudPluginInterface pluginInterface)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        PluginHandlers.Initialise(ref pluginInterface, this);
        Services = new AcquisitionSevices();

        AcquistionParser = new AcquisitionParser(Services.Sheets);
        Translator.Initialise(Services.Configuration);

        NetworkClient = new NetworkClient();

        DirtyHandler = new DirtyHandler();
        SaveHandler = new SaveHandler(DirtyHandler, Services.Configuration);

        Database = new DatableDatabase(Services, DirtyHandler);
        UserList = new UserList(Database, DirtyHandler);

        LodestoneNetworker = new LodestoneNetworker(NetworkClient, DirtyHandler, Services.Configuration);

        ImageReader = new ImageReader();
        ImageDatabase = new ImageDatabase.ImageDatabase(LodestoneNetworker, ImageReader, AcquistionParser.LodestoneIDParser, NetworkClient);

        HookHandler = new HookHandler(Services, UserList, Database, DirtyHandler);
        UpdateHandler = new UpdateHandler(LodestoneNetworker, ImageDatabase, UserList, AcquistionParser, SaveHandler, HookHandler);
        AcquirerHandler = new AcquirerHandler(Services, LodestoneNetworker, AcquistionParser);
        WindowHandler = new WindowHandler(Services, UserList, Database, AcquirerHandler, LodestoneNetworker, DirtyHandler, AcquistionParser, ImageDatabase);

        CommandHandler = new CommandHandler(Services.Configuration, WindowHandler);

        Services.Configuration.Initialise(Database);
    }

    public void Dispose()
    {
        SafeDispose(NetworkClient.CancelPendingRequests);
        SafeDispose(NetworkClient.Dispose);
        SafeDispose(ImageReader.Dispose);
        SafeDispose(ImageDatabase.Dispose);
        SafeDispose(AcquistionParser.Dispose);
        SafeDispose(CommandHandler.Dispose);
        SafeDispose(HookHandler.Dispose);
        SafeDispose(WindowHandler.Dispose);
        SafeDispose(UpdateHandler.Dispose);
        SafeDispose(SaveHandler.Dispose);
        SafeDispose(AcquirerHandler.Dispose);
        SafeDispose(LodestoneNetworker.Dispose);
    }

    void SafeDispose(Action disposeAction)
    {
        try
        {
            disposeAction.Invoke();
        }
        catch (Exception ex)
        {
            PluginHandlers.PluginLog.Error(ex, "Exception during dispose");
        }
    }
}
