using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Hooking.Interfaces;
using AcquisitionDate.Services.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking;

internal class HookHandler : IHookHandler
{
    readonly IUserList UserList;
    readonly IAcquisitionServices Services;

    public HookHandler(IAcquisitionServices services, IUserList userList)
    {
        Services = services;
        UserList = userList;

        _Register();
    }

    void _Register()
    {
        Register(new CharacterManagerHook(UserList));
        Register(new AchievementWindowHook(Services, UserList));
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
