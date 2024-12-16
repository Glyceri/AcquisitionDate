using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Updating.Updatables;

internal class UnlockHooksHelper : IUpdatable
{
    readonly IUnlocksHook UnlockHooks;

    public UnlockHooksHelper(IUnlocksHook unlocksHook)
    {
        UnlockHooks = unlocksHook;
    }

    public void Update(float deltaTime)
    {
        UnlockHooks.Update();
    }
}
