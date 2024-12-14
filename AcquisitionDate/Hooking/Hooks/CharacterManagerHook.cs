using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class CharacterManagerHook : HookableElement
{
    delegate BattleChara* BattleChara_OnInitializeDelegate(BattleChara* battleChara);
    delegate BattleChara* BattleChara_TerminateDelegate(BattleChara* battleChara);
    delegate BattleChara* BattleChara_DestroyDelegate(BattleChara* battleChara, bool freeMemory);

    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B F9 E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B D7", DetourName = nameof(InitializeBattleChara))]
    readonly Hook<BattleChara_OnInitializeDelegate>? OnInitializeBattleCharaHook = null;

    [Signature("40 53 48 83 EC 20 8B 91 ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ??", DetourName = nameof(TerminateBattleChara))]
    readonly Hook<BattleChara_TerminateDelegate>? OnTerminateBattleCharaHook = null;

    [Signature("48 89 5C 24 08 57 48 83 EC 20 48 8D 05 ?? ?? ?? ?? 48 8B F9 48 89 01 8B DA 48 8D 05 ?? ?? ?? ?? 48 89 81 A0 01 00 00 48 81 C1 90 36 00 00", DetourName = nameof(DestroyBattleChara))]
    readonly Hook<BattleChara_DestroyDelegate>? OnDestroyBattleCharaHook = null;

    readonly IUserList UserList;

    public CharacterManagerHook(IUserList userList)
    {
        UserList = userList;
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

    BattleChara* InitializeBattleChara(BattleChara* bChara)
    {
        BattleChara* initializedBattleChara = OnInitializeBattleCharaHook!.Original(bChara);

        PluginHandlers.Framework.Run(() => HandleAsCreated(bChara));

        return initializedBattleChara;
    }

    BattleChara* TerminateBattleChara(BattleChara* bChara)
    {
        HandleAsDeleted(bChara);

        return OnTerminateBattleCharaHook!.Original(bChara);
    }

    BattleChara* DestroyBattleChara(BattleChara* bChara, bool freeMemory)
    {
        HandleAsDeleted(bChara);

        return OnDestroyBattleCharaHook!.Original(bChara, freeMemory);
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
