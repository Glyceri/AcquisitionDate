using AcquisitionDate.Serializiation.DirtySystem.Interfaces;
using System;

namespace AcquisitionDate.Serializiation.DirtySystem;

internal class DirtyHandler : IDirtySetter, IDirtyListener
{
    Action? OnDirty = () => { };

    public void NotifyDirty()
    {
        OnDirty?.Invoke();
    }

    public void RegisterDirty(Action onDirty)
    {
        OnDirty -= onDirty;
        OnDirty += onDirty;
    }

    public void UnregisterDirty(Action onDirty)
    {
        OnDirty -= OnDirty;
    }
}
