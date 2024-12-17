using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Updating.Interfaces;

namespace AcquisitionDate.Hooking.Hooks;

internal abstract class UnlockHook : HookableElement, IUpdatable
{
    protected IDirtyListener DirtyListener;
    protected IUserList UserList;

    public UnlockHook(IDirtyListener dirtyListener, IUserList userList)
    {
        DirtyListener = dirtyListener;
        UserList = userList;

        DirtyListener.RegisterDirtyUser(Reset);
    }

    public void Update(float deltaTime)
    {
        if (UserList.ActiveUser == null) return;
        
        OnUpdate(deltaTime);
    }

    public abstract void OnUpdate(float deltaTime);
    public abstract void Reset();
}
