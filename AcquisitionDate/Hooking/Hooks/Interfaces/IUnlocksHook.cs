using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.Interfaces;

internal interface IUnlocksHook : IHookableElement, IUpdatable
{
    void Update(float deltaTime);
}
