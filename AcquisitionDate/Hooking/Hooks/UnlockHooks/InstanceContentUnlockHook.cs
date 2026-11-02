using AcquisitionDate.Database.Enums;
using AcquisitionDate.Services.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Hooking.Enums;
using Dalamud.Game.DutyState;
using Lumina.Excel;

namespace AcquisitionDate.Hooking.Hooks.UnlockHooks;

internal class InstanceContentUnlockHook : UnlockHook
{
    private readonly List<uint> _instancedContentCompleted = [];

    public InstanceContentUnlockHook(IDatabase database, ISheets sheets) 
        : base(database, sheets) 
        { }

    public override void Init()
    {
        PluginHandlers.DutyState.DutyCompleted += OnDutyCompleted;
    }
    
    public override void Dispose()
    {
        PluginHandlers.DutyState.DutyCompleted -= OnDutyCompleted;
    }

    public override void Reset()
    {
        _instancedContentCompleted.Clear();

        foreach (ContentFinderCondition iContent in Sheets.AllContentFinderConditions)
        {
            ContentLinkType contentLinkType = (ContentLinkType)iContent.ContentLinkType;
        
            if (contentLinkType != ContentLinkType.InstanceContent)
            {
                continue;   
            }
            
            uint contentRowId = iContent.Content.RowId;
            
            if (!UIState.IsInstanceContentCompleted(contentRowId))
            {
                continue;
            }

            _instancedContentCompleted.Add(contentRowId);
        }
    }

    private void OnDutyCompleted(IDutyStateEventArgs args)
    {
        RowRef<ContentFinderCondition> contentFinderCondition = args.ContentFinderCondition;
        ContentLinkType                contentLinkType        = (ContentLinkType)contentFinderCondition.Value.ContentLinkType;
        
        if (contentLinkType != ContentLinkType.InstanceContent)
        {
            return;   
        }
        
        string ccName       = contentFinderCondition.Value.Name.ExtractText();
        uint   contentRowId = contentFinderCondition.Value.Content.RowId;
        bool   completed    = _instancedContentCompleted.Contains(contentRowId);
        
        PluginHandlers.PluginLog.Information($"Detected [duty]{ccName} completed with the ContentFinderConditionId: {contentFinderCondition.RowId}, ContentId: {contentFinderCondition.Value.Content.RowId}, territory: {args.TerritoryType.RowId}");
        
        IDatableData? localUser = Database.GetLocalUser();
        
        if (localUser == null)
        {
            return;
        }
        
        IDatableList  dateList         = localUser.GetDateList(AcquirableDateType.Duty);
        UnlockedDate? date             = dateList.GetDate(contentRowId);
        bool          hasLodestoneDate = date?.GetSpecificDateTime(AcquiredDateType.Lodestone) != null;
        
        if (date == null && completed)
        {
            dateList.SetDate(contentRowId, DateTime.Now, AcquiredDateType.Lodestone);
        }
        else if (date == null && !completed)
        {
            dateList.SetDate(contentRowId, DateTime.Now, AcquiredDateType.Manual);
        }
        else if (date != null && !hasLodestoneDate)
        {
            dateList.SetDate(contentRowId, DateTime.Now, AcquiredDateType.Lodestone);
        }
    }

    public override void Update(float deltaTime) // No update needed for this one
        { } 
}
