using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal abstract class UnlockHook : HookableElement, IUpdatable
{
    protected IUserList UserList;
    protected ISheets Sheets;

    public UnlockHook(IUserList userList, ISheets sheets)
    {
        UserList = userList;
        Sheets = sheets;
    }

    public abstract void Update(float deltaTime);
    public abstract void Reset();
}
