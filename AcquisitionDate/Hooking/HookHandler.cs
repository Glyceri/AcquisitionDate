using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Hooking.Hooks.ATKHooks;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking;

internal class HookHandler : IHookHandler
{
    public IUnlocksHook UnlocksHook { get; private set; } = null!;

    readonly IAcquisitionServices Services;
    readonly IDirtyListener DirtyListener;
    readonly IDatabase Database;

    public HookHandler(IAcquisitionServices services, IDatabase database, IDirtyListener dirtyListener)
    {
        Services      = services;
        Database      = database;
        DirtyListener = dirtyListener;

        _Register();
    }

    void _Register()
    {
        Register(new AchievementWindowHook(Database, Services));
        Register(new QuestJournalWindowHook(Database, Services));
        Register(new CutsceneReplayWindowHook(Database, Services));
        Register(new EorzeaIncognitaWindowHook(Database, Services));
        Register(new CharacterClassWindowHook(Database, Services));
        Register(new FishGuideWindowHook(Database, Services));
        Register(new MountWindowHook(Database, Services));
        Register(new MinionWindowHook(Database, Services));
        Register(new OrnamentWindowHook(Database, Services));
        Register(new GlassSelectWindowHook(Database, Services));
        Register(new OrchestrionWindowHook(Database, Services));
        Register(UnlocksHook = new UnlocksHook(Services.Sheets, Database, DirtyListener));
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
