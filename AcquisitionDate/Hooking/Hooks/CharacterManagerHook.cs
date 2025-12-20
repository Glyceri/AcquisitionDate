using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class CharacterManagerHook : HookableElement
{
    private readonly Hook<BattleChara.Delegates.OnInitialize>   OnInitializeBattleCharaHook;
    private readonly Hook<BattleChara.Delegates.Terminate>      OnTerminateBattleCharaHook;
    private readonly Hook<BattleChara.Delegates.Dtor>           OnDestroyBattleCharaHook;

    readonly IUserList UserList;
    readonly IAcquisitionServices Services;

    public CharacterManagerHook(IUserList userList, IAcquisitionServices services)
    {
        UserList       = userList;
        Services       = services;

        OnInitializeBattleCharaHook = PluginHandlers.Hooking.HookFromAddress<BattleChara.Delegates.OnInitialize>   ((nint)BattleChara.StaticVirtualTablePointer->OnInitialize,     InitializeBattleChara);
        OnTerminateBattleCharaHook  = PluginHandlers.Hooking.HookFromAddress<BattleChara.Delegates.Terminate>      ((nint)BattleChara.StaticVirtualTablePointer->Terminate,        TerminateBattleChara);
        OnDestroyBattleCharaHook    = PluginHandlers.Hooking.HookFromAddress<BattleChara.Delegates.Dtor>           ((nint)BattleChara.StaticVirtualTablePointer->Dtor,             DestroyBattleChara);
    }

    public override void Init()
    {
        OnInitializeBattleCharaHook?.Enable();
        OnTerminateBattleCharaHook?.Enable();
        OnDestroyBattleCharaHook?.Enable();

        FloodInitialList();
    }


    void FloodInitialList()
    {
        BattleChara* bChara = CharacterManager.Instance()->BattleCharas[0];
        if (bChara == null) return;

        HandleAsCreated(bChara);
    }

    void InitializeBattleChara(BattleChara* bChara)
    {
        try
        {
            OnInitializeBattleCharaHook!.OriginalDisposeSafe(bChara);
        }
        catch (Exception e)
        {
            Services.PetLog.LogException(e);
        }

        PluginHandlers.Framework.Run(() => HandleAsCreated(bChara));
    }

    void TerminateBattleChara(BattleChara* bChara)
    {
        HandleAsDeleted(bChara);

        try
        {
            OnTerminateBattleCharaHook!.OriginalDisposeSafe(bChara);
        }
        catch (Exception e)
        {
            Services.PetLog.LogException(e);
        }
    }

    GameObject* DestroyBattleChara(BattleChara* bChara, byte freeMemory)
    {
        HandleAsDeleted(bChara);

        try
        {
            return OnDestroyBattleCharaHook!.OriginalDisposeSafe(bChara, freeMemory);
        }
        catch (Exception e)
        {
            Services.PetLog.LogException(e);
        }

        return null;
    }

    void HandleAsCreated(BattleChara* newBattleChara)
    {
        if (newBattleChara->ObjectIndex != 0) return;

        HandleAsDeleted(newBattleChara);

        UserList.Create(newBattleChara);
    }

    void HandleAsDeleted(BattleChara* bChara)
    {
        if (bChara->ObjectIndex != 0) return;

        UserList.Delete(bChara);
    }

    public override void Dispose()
    {
        OnInitializeBattleCharaHook?.Dispose();
        OnTerminateBattleCharaHook?.Dispose();
        OnDestroyBattleCharaHook?.Dispose();
    }
}
