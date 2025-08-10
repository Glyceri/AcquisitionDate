using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class LevelupUnlockHook : UnlockHook
{
    private short[] currentClassJobLevels = [];

    public LevelupUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        PluginHandlers.ClientState.LevelChanged += OnLevelChanged;
    }

    public override void Reset()
    {
        currentClassJobLevels = PlayerState.Instance()->ClassJobLevels.ToArray();
    }

    void OnLevelChanged(uint classJobId, uint level)
    {
        PluginHandlers.PluginLog.Verbose($"Detected a level change on the job: {classJobId} to level: {level}");

        ClassJob? classJob = Sheets.GetClassJob(classJobId);
        if (classJob == null)
        {
            PluginHandlers.PluginLog.Information("Couldn't find the classjob in the sheets???? HOW");
            return;
        }

        sbyte arrayIndex = classJob.Value.ExpArrayIndex;
        if (arrayIndex < 0 || arrayIndex >= currentClassJobLevels.Length)
        {
            PluginHandlers.PluginLog.Information($"Array index is out of range: {arrayIndex} on the classJobArray: {currentClassJobLevels.Length}");
            return;
        }

        short currentLevel = currentClassJobLevels[arrayIndex];
        if (currentLevel >= level)
        {
            PluginHandlers.PluginLog.Information($"This resulted in no actual change.");
            return;
        }

        Reset();

        PluginHandlers.PluginLog.Information($"The class: {classJobId}, {arrayIndex}, {classJob.Value.Name.ExtractText()} leveled up from: {currentLevel} to {level}. This has been marked.");

        uint preparedClassJobID = (uint)arrayIndex * 10000;
        preparedClassJobID += level;

        // Because I can only have one indexer im storing multiple pieces of data in one number, the 10000 part is the class job, the small number is the level

        UserList.ActiveUser?.Data.GetDate(AcquirableDateType.ClassLVL).SetDate(preparedClassJobID, DateTime.Now, AcquiredDateType.Manual);
    }

    public override void Dispose()
    {
        PluginHandlers.ClientState.LevelChanged -= OnLevelChanged;
    }

    public override void Update(float deltaTime) { } // No update needed for this one
}
