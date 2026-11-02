using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Updating.Interfaces;
using System;

namespace AcquisitionDate.Serializiation;

internal class SaveHandler : IDisposable, IUpdatable
{
    private readonly IDirtyListener DirtyListener;
    private readonly Configuration  Configuration;

    private bool _shouldSave;

    public SaveHandler(IDirtyListener dirtyListener, Configuration configuration)
    {
        DirtyListener = dirtyListener;
        Configuration = configuration;

        DirtyListener.RegisterDirtyDatabase(OnDirty);
    }

    public void Dispose()
    {
        DirtyListener.UnregisterDirtyDatabase(OnDirty);
    }
    
    private void OnDirty()
    {
        _shouldSave = true;
    }

    public void Update(float deltaTime)
    {
        if (!_shouldSave)
        {
            return;
        }

        _shouldSave = false;
        
        Configuration.Save();
    }
}
