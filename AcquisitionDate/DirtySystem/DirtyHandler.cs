using AcquisitionDate.DirtySystem.Interfaces;
using System;

namespace AcquisitionDate.DirtySystem;

internal class DirtyHandler : IDirtySetter, IDirtyListener
{
    Action? OnDirty = () => { };
    Action? OnDirtyUser = () => { };

    public void NotifyDirtyDatabase()
    {
        OnDirty?.Invoke();
    }

    public void NotifyDirtyUser()
    {
        OnDirtyUser?.Invoke();
    }

    public void RegisterDirtyDatabase(Action onDirty)
    {
        OnDirty -= onDirty;
        OnDirty += onDirty;
    }

    public void RegisterDirtyUser(Action onDirtyUser)
    {
        OnDirtyUser -= onDirtyUser;
        OnDirtyUser += onDirtyUser;
    }

    public void UnregisterDirtyDatabase(Action onDirty)
    {
        OnDirty -= OnDirty;
    }

    public void UnregisterDirtyUser(Action onDirtyUser)
    {
        OnDirtyUser -= onDirtyUser;
    }
}
