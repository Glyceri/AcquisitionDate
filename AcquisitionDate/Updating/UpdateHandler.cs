using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Serializiation;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using AcquisitionDate.Updating.Updatables;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace AcquisitionDate.Updating;

internal class UpdateHandler : IUpdateHandler
{
    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IImageDatabase ImageDatabase;
    readonly IUserList UserList;
    readonly SaveHandler SaveHandler;
    readonly IHookHandler HookHandler;
    readonly IAcquisitionParser AcquistionParser;

    readonly List<IUpdatable> _updatables = new List<IUpdatable>();

    public UpdateHandler(ILodestoneNetworker lodestoneNetworker, IImageDatabase imageDatabase, IUserList userList, IAcquisitionParser acquistionParser, SaveHandler saveHandler, IHookHandler hookHandler)
    {
        LodestoneNetworker = lodestoneNetworker;
        ImageDatabase = imageDatabase;
        UserList = userList;
        SaveHandler = saveHandler;
        HookHandler = hookHandler;
        AcquistionParser = acquistionParser;

        PluginHandlers.Framework.Update += OnUpdate;
        Setup();
    }

    void Setup()
    {
        _updatables.Add(SaveHandler);
        _updatables.Add(ImageDatabase);
        _updatables.Add(LodestoneNetworker);
        _updatables.Add(HookHandler.UnlocksHook);
        _updatables.Add(new LodestoneIDHelper(AcquistionParser, LodestoneNetworker, UserList));
    }

    public void Dispose()
    {
        PluginHandlers.Framework.Update -= OnUpdate;
    }

    void OnUpdate(IFramework framework)
    {
        float deltaTime = (float)framework.UpdateDelta.TotalSeconds;

        int updatableCount = _updatables.Count;
        for (int i = 0; i < updatableCount; i++)
        {
            IUpdatable updatable = _updatables[i];
            updatable.Update(deltaTime);
        }
    }
}
