using System;

namespace AcquisitionDate.Serializiation.DirtySystem.Interfaces;

internal interface IDirtyListener
{
    void RegisterDirty(Action onDirty);

    void UnregisterDirty(Action onDirty);
}
