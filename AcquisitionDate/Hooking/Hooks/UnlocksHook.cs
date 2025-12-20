using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Hooking.Hooks.UnlockHooks;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class UnlocksHook : HookableElement, IUnlocksHook
{
    readonly List<UnlockHook> unlockHooks = new List<UnlockHook>();

    readonly ISheets Sheets;
    readonly IUserList UserList;
    readonly IDirtyListener DirtyListener;

    public UnlocksHook(ISheets sheets, IUserList userList, IDirtyListener dirtyListener)
    {
        Sheets = sheets;
        UserList = userList;
        DirtyListener = dirtyListener;
    }

    public override void Init()
    {
        DirtyListener.RegisterDirtyUser(Reset);

        Register(new AchievementUnlockHook(UserList, Sheets));
        Register(new FishyUnlockHook(UserList, Sheets));
        Register(new InstanceContentUnlockHook(UserList, Sheets));
        Register(new ItemUnlockHook(UserList, Sheets));
        Register(new LevelupUnlockHook(UserList, Sheets));
        Register(new QuestUnlockHook(UserList, Sheets));
        Register(new EorzeaIncognitaUnlockHook(UserList, Sheets));
    }

    void Register(UnlockHook unlockHook)
    {
        unlockHooks.Add(unlockHook);

        unlockHook.Init();
        unlockHook.Reset();
    }

    public void Update(float deltaTime)
    {
        if (UserList.ActiveUser == null) return;

        foreach (UnlockHook unlockHook in unlockHooks)
        {
            unlockHook.Update(deltaTime);
        }
    }

    void Reset()
    {
        foreach (UnlockHook unlockHook in unlockHooks)
        {
            unlockHook.Reset();
        }
    }

    public override void Dispose()
    {
        DirtyListener.UnregisterDirtyUser(Reset);

        foreach (UnlockHook unlockHook in unlockHooks)
        {
            unlockHook.Dispose();
        }

        unlockHooks.Clear();
    }
}
