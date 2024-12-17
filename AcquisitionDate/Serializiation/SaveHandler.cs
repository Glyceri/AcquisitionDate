using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using System;

namespace AcquisitionDate.Serializiation;

internal class SaveHandler : IDisposable, IUpdatable
{
    readonly IDirtyListener DirtyListener;
    readonly Configuration Configuration;

    bool shouldSave = false;

    public SaveHandler(IDirtyListener dirtyListener, Configuration configuration)
    {
        DirtyListener = dirtyListener;
        Configuration = configuration;

        DirtyListener.RegisterDirtyDatabase(OnDirty);
    }

    void OnDirty()
    {
        shouldSave = true;
    }

    public void Update(float deltaTime)
    {
        if (!shouldSave) return;

        shouldSave = false;
        Configuration.Save();
    }

    public void Dispose()
    {
        DirtyListener.UnregisterDirtyDatabase(OnDirty);
    }
}
