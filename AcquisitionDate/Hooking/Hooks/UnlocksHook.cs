using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Hooking.Hooks.UnlockHooks;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;
using AcquisitionDate.Database.Interfaces;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class UnlocksHook : HookableElement, IUnlocksHook
{
    readonly List<UnlockHook> unlockHooks = new List<UnlockHook>();

    readonly ISheets Sheets;
    readonly IDatabase Database;
    readonly IDirtyListener DirtyListener;

    public UnlocksHook(ISheets sheets, IDatabase database, IDirtyListener dirtyListener)
    {
        Sheets        = sheets;
        Database      = database;
        DirtyListener = dirtyListener;
    }

    public override void Init()
    {
        DirtyListener.RegisterDirtyUser(Reset);

        Register(new AchievementUnlockHook(Database, Sheets));
        Register(new FishyUnlockHook(Database, Sheets));
        Register(new InstanceContentUnlockHook(Database, Sheets));
        Register(new ItemUnlockHook(Database, Sheets));
        Register(new LevelupUnlockHook(Database, Sheets));
        Register(new QuestUnlockHook(Database, Sheets));
        Register(new EorzeaIncognitaUnlockHook(Database, Sheets));
    }

    void Register(UnlockHook unlockHook)
    {
        unlockHooks.Add(unlockHook);

        unlockHook.Init();
        unlockHook.Reset();
    }

    public void Update(float deltaTime)
    {
        if (Database.GetLocalUser() == null) return;

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
