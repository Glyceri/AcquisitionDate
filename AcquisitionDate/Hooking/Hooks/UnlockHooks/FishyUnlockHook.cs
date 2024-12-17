using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class FishyUnlockHook : UnlockHook
{
    readonly List<uint> UnlockedFishies = new List<uint>();

    delegate IntPtr FishyCaughtDelegate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, byte unk9, byte unk10, byte unk11, byte unk12);

    [Signature("40 55 56 41 54 41 56 41 57 48 8D 6C 24 ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 F7", DetourName = nameof(FishyCaughtDetour))]
    readonly Hook<FishyCaughtDelegate>? FishyCaughtHook;

    public FishyUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        PluginHandlers.PluginLog.Error("The unlocked fishies and spear fishies part are just simply not accurate at all! Reimplement it!");
        //FishyCaughtHook?.Enable();
    }

    public override void Reset()
    {
        UnlockedFishies.Clear();

        foreach (FishParameter fish in Sheets.AllFishies)
        {
            if (!fish.IsInLog) continue;
            if (!IsFishUnlocked(fish)) continue;

            UnlockedFishies.Add(fish.RowId);
        }
    }

    bool IsFishUnlocked(FishParameter fish)
    {
        uint fishID = fish.RowId;

        int offset = (int)fishID / 8;
        int bit = (byte)fishID % 8;

        return ((PlayerState.Instance()->CaughtFishBitmask[offset] >> bit) & 1) == 1;
    }

    IntPtr FishyCaughtDetour(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, byte unk9, byte unk10, byte unk11, byte unk12)
    {
        if (!UnlockedFishies.Contains(fishId))
        {
            UnlockedFishies.Add(fishId);

            PluginHandlers.PluginLog.Verbose($"Fishy with ID {fishId} has been found and registered.");
            UserList.ActiveUser?.Data.FishingList.SetDate(fishId, DateTime.Now, AcquiredDateType.Manual);
        }

        return FishyCaughtHook!.Original(module, fishId, large, size, amount, level, unk7, unk8, unk9, unk10, unk11, unk12);
    }

    public override void Dispose()
    {
        FishyCaughtHook?.Dispose();
    }

    public override void Update(float deltaTime) { } // No update needed for this one
}
