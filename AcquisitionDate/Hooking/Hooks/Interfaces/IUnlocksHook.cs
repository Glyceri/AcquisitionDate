using AcquisitionDate.Hooking.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.Interfaces;

internal interface IUnlocksHook : IHookableElement
{
    void Update();
}
