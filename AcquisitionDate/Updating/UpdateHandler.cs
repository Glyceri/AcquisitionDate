using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
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
    readonly IUserList UserList;
    readonly IAcquisitionServices Services;
    readonly SaveHandler SaveHandler;

    readonly List<IUpdatable> _updatables = new List<IUpdatable>();

    public UpdateHandler(ILodestoneNetworker lodestoneNetworker, IUserList userList, IAcquisitionServices services, SaveHandler saveHandler)
    {
        LodestoneNetworker = lodestoneNetworker;
        UserList = userList;
        Services = services;
        SaveHandler = saveHandler;

        PluginHandlers.Framework.Update += OnUpdate;
        Setup();
    }

    void Setup()
    {
        _updatables.Add(new LodestoneNetworkHelper(LodestoneNetworker));
        _updatables.Add(new LodestoneIDHelper(LodestoneNetworker, UserList, Services.Sheets));
        _updatables.Add(new SaveHandlerHelper(SaveHandler));
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
