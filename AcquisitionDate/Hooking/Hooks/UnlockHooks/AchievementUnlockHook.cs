using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class AchievementUnlockHook : UnlockHook
{
    delegate void OnAchievementUnlockDelegate(Achievement* achievement, uint achievementID);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockingHook;

    public AchievementUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        AchievementUnlockingHook?.Enable();
    }

    public override void Dispose()
    {
        AchievementUnlockingHook?.Dispose();
    }

    void AchievementUnlockedDetour(Achievement* achievement, uint achievementID)
    {
        AchievementUnlockingHook!.Original(achievement, achievementID);

        PluginHandlers.PluginLog.Verbose($"Detected Acquired Achievement with ID: {achievementID}");
        UserList.ActiveUser?.Data.AchievementList.SetDate(achievementID, DateTime.Now, AcquiredDateType.Manual);
    }

    public override void Reset() { } // No Reset needed for achievements due to the direct hook.
    public override void Update(float deltaTime) { } // No Update needed for achievements due to the direct hook.
}
