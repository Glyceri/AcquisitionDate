using System;

namespace AcquisitionDate.Hooking.Interfaces;

internal interface IHookableElement : IDisposable
{
    void Init();
}
