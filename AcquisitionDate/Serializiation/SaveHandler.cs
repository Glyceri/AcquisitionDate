using AcquisitionDate.Serializiation.DirtySystem.Interfaces;
using System;

namespace AcquisitionDate.Serializiation;

internal class SaveHandler : IDisposable
{
    readonly IDirtyListener DirtyListener;
    readonly Configuration Configuration;

    public SaveHandler(IDirtyListener dirtyListener, Configuration configuration)
    {
        DirtyListener = dirtyListener;
        Configuration = configuration;

        DirtyListener.RegisterDirty(OnDirty);
    }

    void OnDirty()
    {
        Configuration.Save();
    }

    public void Dispose()
    {
        DirtyListener.UnregisterDirty(OnDirty);
    }
}
