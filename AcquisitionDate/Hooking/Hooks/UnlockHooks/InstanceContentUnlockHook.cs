using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using Dalamud.Game.DutyState;
using Lumina.Excel;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal unsafe class InstanceContentUnlockHook : UnlockHook
{
    private readonly List<uint> InstancedContentCompleted = new List<uint>();

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
            
            if (!UIState.IsInstanceContentCompleted(contentRowID))
            {
                continue;
            }

            InstancedContentCompleted.Add(contentRowID);
        }
    }

    private void OnDutyCompleted(IDutyStateEventArgs args)
    {
        RowRef<ContentFinderCondition> contentFinderCondition = args.ContentFinderCondition;
        
        string ccName = contentFinderCondition.Value.Name.ExtractText();
        
        bool instanceHasAlreadyBeenCompleted = InstancedContentCompleted.Contains(contentFinderCondition.Value.Content.RowId);
        
        PluginHandlers.PluginLog.Information($"Detected a duty completed with the ID: {args.TerritoryType.RowId} with the detected ContentFinderConditionID: {contentFinderCondition.RowId} with the name: {ccName}");
        
        if (instanceHasAlreadyBeenCompleted)
        {
            PluginHandlers.PluginLog.Information("This instance has already been completed however.");
            return;
        }

        UserList.ActiveUser?.Data.GetDate(AcquirableDateType.Duty).SetDate(contentFinderCondition.RowId, DateTime.Now, AcquiredDateType.Manual);
    }

    public override void Dispose()
    {
        PluginHandlers.DutyState.DutyCompleted -= OnDutyCompleted;
    }

    public override void Update(float deltaTime) { } // No update needed for this one
}
