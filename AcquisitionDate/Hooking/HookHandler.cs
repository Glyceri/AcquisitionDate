using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Hooking.Hooks.Interfaces;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking;

internal class HookHandler : IHookHandler
{
    public IUnlocksHook UnlocksHook { get; private set; } = null!;

    readonly IUserList UserList;
    readonly IAcquisitionServices Services;
    readonly IDirtyListener DirtyListener;

    public HookHandler(IAcquisitionServices services, IUserList userList, IDirtyListener dirtyListener)
    {
        Services = services;
        UserList = userList;
        DirtyListener = dirtyListener;

        _Register();
    }

    void _Register()
    {
        Register(new CharacterManagerHook(UserList));
        Register(new AchievementWindowHook(Services, UserList));
        Register(new QuestJournalWindowHook(Services, UserList));
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
            hookableElement.Dispose();
        }
    }
}
