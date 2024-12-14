using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Hooking.Interfaces;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking;

internal class HookHandler : IHookHandler
{
    readonly IUserList UserList;

    public HookHandler(IUserList userList)
    {
        UserList = userList;

        _Register();
    }

    void _Register()
    {
        Register(new CharacterManagerHook(UserList));
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
