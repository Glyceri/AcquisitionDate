using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Hooking.Interfaces;

namespace AcquisitionDate.Hooking;

internal abstract class HookableElement : IHookableElement
{
    public HookableElement()
    {
        PluginHandlers.Hooking.InitializeFromAttributes(this);
    }

    public abstract void Init();
    public abstract void Dispose();
}
