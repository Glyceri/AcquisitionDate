using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Hooking.Hooks.ATKHooks;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Hooking.Hooks.UnlockHooks;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking;

internal class HookHandler : IHookHandler
{
    public IUnlocksHook UnlocksHook { get; private set; } = null!;

    readonly IUserList UserList;
    readonly IAcquisitionServices Services;
    readonly IDirtyListener DirtyListener;
    readonly IDatabase Database;

    public HookHandler(IAcquisitionServices services, IUserList userList, IDatabase database, IDirtyListener dirtyListener)
    {
        Services = services;
        UserList = userList;
        Database = database;
        DirtyListener = dirtyListener;

        _Register();
    }

    void _Register()
    {
        Register(new CharacterManagerHook(UserList, Services));
        Register(new AchievementWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new QuestJournalWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new CutsceneReplayWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new EorzeaIncognitaWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new CharacterClassWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new FishGuideWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new MountWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new MinionWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new OrnamentWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new GlassSelectWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(new OrchestrionWindowHook(UserList, Database, Services.Sheets, Services.Configuration));
        Register(UnlocksHook = new UnlocksHook(Services.Sheets, UserList, DirtyListener));
    }

    readonly List<IHookableElement> hookableElements = new List<IHookableElement>();

    void Register(IHookableElement element)
    {
        hookableElements.Add(element);
        element?.Init();
    }

    public void Dispose()
    {
        foreach (IHookableElement hookableElement in hookableElements)
        {
            try
            {
                hookableElement.Dispose();
            }
            catch (Exception e)
            {
                PluginHandlers.PluginLog.Error(e, "Failed to dispose of a hook.");
            }
        }
    }
}
