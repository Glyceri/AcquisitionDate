using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal abstract class UnlockHook : HookableElement, IUpdatable
{
    protected readonly IDatabase Database;
    protected readonly ISheets   Sheets;

    protected UnlockHook(IDatabase database, ISheets sheets)
    {
        Database = database;
        Sheets   = sheets;
    }

    public abstract void Update(float deltaTime);
    public abstract void Reset();
}
