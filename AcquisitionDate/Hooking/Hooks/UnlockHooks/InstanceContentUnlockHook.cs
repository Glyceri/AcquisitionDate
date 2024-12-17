using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class InstanceContentUnlockHook : UnlockHook
{
    readonly List<uint> InstancedContentCompleted = new List<uint>();

    public InstanceContentUnlockHook(IUserList userList, ISheets sheets) : base(userList, sheets) { }

    public override void Init()
    {
        PluginHandlers.DutyState.DutyCompleted += OnDutyCompleted;
    }

    public override void Reset()
    {
        InstancedContentCompleted.Clear();

        foreach (ContentFinderCondition iContent in Sheets.AllContentFinderConditions)
        {
            uint contentRowID = iContent.Content.RowId;
            if (!UIState.IsInstanceContentCompleted(contentRowID)) continue;

            InstancedContentCompleted.Add(contentRowID);
        }
    }

    void OnDutyCompleted(object? sender, ushort dutyID)
    {
        ushort contentFinderConditionID = GameMain.Instance()->CurrentContentFinderConditionId;
        bool instanceHasAlreadyBeenCompleted = true;

        ContentFinderCondition? contentFinderCondition = Sheets.GetContentFinderCondition(contentFinderConditionID);
        string ccName = "[ERROR]";
        if (contentFinderCondition != null)
        {
            ccName = contentFinderCondition.Value.Name.ExtractText();
            instanceHasAlreadyBeenCompleted = InstancedContentCompleted.Contains(contentFinderCondition.Value.Content.RowId);
        }

        PluginHandlers.PluginLog.Verbose($"Detected a duty completed with the ID: {dutyID} with the detected ContentFinderConditionID: {contentFinderConditionID} with the name: {ccName}");
        if (instanceHasAlreadyBeenCompleted)
        {
            PluginHandlers.PluginLog.Verbose("This instance has already been completed however.");
            return;
        }

        UserList.ActiveUser?.Data.DutyList.SetDate(contentFinderConditionID, DateTime.Now, AcquiredDateType.Manual);
    }

    public override void Dispose()
    {
        PluginHandlers.DutyState.DutyCompleted -= OnDutyCompleted;
    }

    public override void Update(float deltaTime) { } // No update needed for this one
}
