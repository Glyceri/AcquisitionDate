using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Hooking;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class AchievementUnlockHook : UnlockHook
{
    private delegate void OnAchievementUnlockDelegate(Achievement* achievement, uint achievementID);

    [Signature("81 FA ?? ?? ?? ?? 0F 87 ?? ?? ?? ?? 53", DetourName = nameof(AchievementUnlockedDetour))]
    private readonly Hook<OnAchievementUnlockDelegate>? AchievementUnlockingHook = null;

    private readonly List<uint> SquareEnixSillyAchievements =
    [
        3811, // Some deep dungeon achievement that gets granted EVERY. SINGLE. LOGIN.
    ];

    public AchievementUnlockHook(IUserList userList, ISheets sheets)
        : base(userList, sheets) { }

    public override void Init()
    {
        AchievementUnlockingHook?.Enable();
    }

    public override void Dispose()
    {
        AchievementUnlockingHook?.Dispose();
    }

    private bool IsAchievementCompleted(Achievement* achievement, uint achievementId)
    {
        if (achievement == null)
        {
            return true;
        }

        // This is a check the decomp code does
        if (achievementId > Sheets.AllAchievements.Length)
        {
            return true;
        }

        // Like the decomp code
        int byteIndex = (int)achievementId >> 3;
        int bitMask = 1 << ((int)achievementId & 7);

        int achievementStatus = (achievement->CompletedAchievements[byteIndex] & bitMask);

        return achievementStatus != 0;
    }

    private bool IsAchievementSilly(Achievement* achievement, uint achievementId)
    {
        if (achievement == null)
        {
            return true;
        }

        if (!SquareEnixSillyAchievements.Contains(achievementId))
        {
            return false;
        }

        return true;
    }

    private void AchievementUnlockedDetour(Achievement* achievement, uint achievementId)
    {
        AchievementUnlockingHook!.Original(achievement, achievementId);

        if (IsAchievementSilly(achievement, achievementId))
        {
            return;
        }

        if (IsAchievementCompleted(achievement, achievementId))
        {
            return;
        }

        PluginHandlers.PluginLog.Information($"Detected Acquired Achievement with ID: [{achievementId}].{Environment.NewLine}This achievement has the name: [{Sheets.GetAchievementByID(achievementId)?.Name.ToDalamudString().TextValue ?? "No achievement name found."}].");

        UserList.ActiveUser?.Data.GetDate(AcquirableDateType.Achievement).SetDate(achievementId, DateTime.Now, AcquiredDateType.Manual);
    }

    public override void Reset() 
        { }

    public override void Update(float deltaTime)
        { }
}
