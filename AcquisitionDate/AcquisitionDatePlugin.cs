using Dalamud.Plugin;
using AcquisitionDate.Windows;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking;
using AcquisitionDate.Updating.Interfaces;
using AcquisitionDate.Updating;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Hooking;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Services;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.Acquisition;
using AcquisitionDate.DirtySystem;
using System.Reflection;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Parser;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.ImageDatabase;
using System;
using System.Threading;
using System.Threading.Tasks;
using AcquisitionDate.Commands;
using AcquisitionDate.Translation;
using Dalamud.IoC;
using KamiToolKit;

namespace AcquisitionDate;

public sealed class AcquisitionDatePlugin : IAsyncDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    
    public string Version;

    internal WindowHandler WindowHandler { get; private set; }

    IImageDatabase ImageDatabase;
    IImageReader ImageReader;
    IAcquisitionParser AcquistionParser;
    IAcquisitionServices Services;
    IUpdateHandler UpdateHandler;
    INetworkClient NetworkClient;
    ILodestoneNetworker LodestoneNetworker;
    IHookHandler HookHandler;

    IDatabase Database;
    IAcquirerHandler AcquirerHandler;

    CommandHandler CommandHandler;
    DirtyHandler DirtyHandler;
    SaveHandler SaveHandler;
    
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        await PluginHandlers.Initialise(PluginInterface, this);
        Services = new AcquisitionSevices();

        await KamiToolKitLibrary.InitializeAsync(PluginInterface, "Acquisition Date");
        
        AcquistionParser = new AcquisitionParser(Services.Sheets);
        Translator.Initialise(Services.Configuration);

        NetworkClient = new NetworkClient();

        DirtyHandler = new DirtyHandler();
        SaveHandler = new SaveHandler(DirtyHandler, Services.Configuration);

        Database = new DatableDatabase(Services, DirtyHandler);

        LodestoneNetworker = new LodestoneNetworker(NetworkClient, DirtyHandler, Services.Configuration);

        ImageReader = new ImageReader();
        ImageDatabase = new ImageDatabase.ImageDatabase(LodestoneNetworker, ImageReader, AcquistionParser.LodestoneIDParser, NetworkClient);

        HookHandler = new HookHandler(Services, Database, DirtyHandler);
        UpdateHandler = new UpdateHandler(LodestoneNetworker, ImageDatabase, Database, AcquistionParser, SaveHandler, HookHandler);
        AcquirerHandler = new AcquirerHandler(Services, LodestoneNetworker, AcquistionParser);
        WindowHandler = new WindowHandler(Services, Database, AcquirerHandler, LodestoneNetworker, DirtyHandler, AcquistionParser, ImageDatabase);

        CommandHandler = new CommandHandler(WindowHandler);

        Services.Configuration.Initialise(Database);
    }
    
    public async ValueTask DisposeAsync()
    {
        await KamiToolKitLibrary.DisposeAsync();
        
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

    private void SafeDispose(Action disposeAction)
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
