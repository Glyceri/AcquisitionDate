using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class FishyUnlockHook : UnlockHook
{
    delegate IntPtr FishyCaughtDelegate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, bool gaveMooch, bool gaveData, byte unk11, byte unk12);

    [Signature("40 55 56 41 54 41 56 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 F7", DetourName = nameof(FishyCaughtDetour))]
    readonly Hook<FishyCaughtDelegate>? FishyCaughtHook;

    public FishyUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        FishyCaughtHook?.Enable();
    }

    IntPtr FishyCaughtDetour(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, bool gaveMooch, bool gaveData, byte unk11, byte unk12)
    {
        PluginHandlers.PluginLog.Verbose($"Fishy: {fishId}, large: {large}, Size: {size}, amount: {amount}, level: {level}, unk7: {unk7}, unk8: {unk8}, Mooch: {gaveMooch}, Gave Data: {gaveData}, unk11: {unk11}, unk12: {unk12}");

        if (gaveData)
        {
            DateTime? dateTime = UserList.ActiveUser?.Data.FishingList.GetDate(fishId);
            if (dateTime == null)
            {
                PluginHandlers.PluginLog.Verbose("As far as I'm aware this fishy hasnt been caught before. It has now been added");
                UserList.ActiveUser?.Data.FishingList.SetDate(fishId, DateTime.Now, AcquiredDateType.Manual);
            }
        }

        return FishyCaughtHook!.Original(module, fishId, large, size, amount, level, unk7, unk8, gaveMooch, gaveData, unk11, unk12);
    }

    public override void Dispose()
    {
        FishyCaughtHook?.Dispose();
    }

    public override void Update(float deltaTime) { } // No update needed for this one
    public override void Reset() { } // No reset needed for this one
}
