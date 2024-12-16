using System;

namespace AcquisitionDate.DirtySystem.Interfaces;

internal interface IDirtyListener
{
    void RegisterDirtyDatabase(Action onDirty);
    void RegisterDirtyUser(Action onDirtyUser);

    void UnregisterDirtyDatabase(Action onDirty);
    void UnregisterDirtyUser(Action onDirtyUser);
}
