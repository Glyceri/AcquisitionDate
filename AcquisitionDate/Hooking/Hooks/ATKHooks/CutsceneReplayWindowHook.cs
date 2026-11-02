using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Hooking.Enums;
using AcquisitionDate.Hooking.Structs;
using AcquisitionDate.Nodes;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class CutsceneReplayWindowHook : DateTextHook
{
    private delegate nint GetJournalCategoryRowDelegate(uint rowIdOrIndex);
    
    [Signature("E8 ?? ?? ?? ?? 48 8B D3 0F B6 48", DetourName = nameof(GetJournalCategoryRowDetour))]
    private readonly Hook<GetJournalCategoryRowDelegate> GetJournalCategoryRowHook = null!;
    
    private CutsceneReplayDateResNode? _cutsceneReplayDateResNode;
    
    private int  _selectedQuest                  = 0;
    private uint _lastGottenJournalCategoryIndex = 0;
    
    public CutsceneReplayWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        GetJournalCategoryRowHook.Enable();
        
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AddonCutSceneReplayDetail", OnRefresh);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,   "AddonCutSceneReplayDetail", OnSetup);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "AddonCutSceneReplayDetail", OnFinalize);
    }

    protected override void OnDispose()
    {
        GetJournalCategoryRowHook.Dispose();
        
        PluginHandlers.AddonLifecycle.UnregisterListener(OnRefresh);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnSetup);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnFinalize);
    }
    
    private nint GetJournalCategoryRowDetour(uint rowIdOrIndex)
    {
        _lastGottenJournalCategoryIndex = rowIdOrIndex;
        
        return GetJournalCategoryRowHook.Original(rowIdOrIndex);
    }
    
    private void OnSetup(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        _cutsceneReplayDateResNode = new CutsceneReplayDateResNode();
        _cutsceneReplayDateResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void OnFinalize(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        _cutsceneReplayDateResNode?.Dispose();
        _cutsceneReplayDateResNode = null;
    }
    
    private void OnRefresh(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (_cutsceneReplayDateResNode == null)
        {
            return;
        }
        
        _cutsceneReplayDateResNode.IsVisible = false;
        
        MyAgentCutsceneReplay* agentStruct = (MyAgentCutsceneReplay*)AgentModule.Instance()->GetAgentByInternalId(AgentId.CutsceneReplay);
        
        if (agentStruct == null)
        {
            return;
        }
        
        _selectedQuest = agentStruct->ActiveQuest;
        
        Services.PetLog.LogVerbose("Selected Index: " + _selectedQuest);
        Services.PetLog.LogVerbose("Selected Journal Entry: " + _lastGottenJournalCategoryIndex);
        
        JournalCategory? journalCategory = Services.Sheets.GetJournalCategory(_lastGottenJournalCategoryIndex);
        
        if (journalCategory == null)
        {
            return;
        }
        
        JournalCategoryDataType dataType = (JournalCategoryDataType)journalCategory.Value.DataType;
        uint                    id       = (uint)_selectedQuest;
        AcquirableDateType      dateType = AcquirableDateType.Quest;
        
        if (dataType != JournalCategoryDataType.Quest && 
            dataType != JournalCategoryDataType.Duty)
        {
            return;
        }
        
        if (dataType == JournalCategoryDataType.Duty)
        {
            dateType = AcquirableDateType.Duty;
        }
        
        bool draw = DrawDateString(_cutsceneReplayDateResNode.DateTextNode, DrawTextFlags.None, dateType, id);
        
        _cutsceneReplayDateResNode.IsVisible = draw;
    }
    
    
    protected override bool HandleConfig(Configuration configuration) 
        => configuration.DrawDatesOnCutsceneReplay;
    
    // TODO: PENDING REMOVAL VVVV
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Quest);

    protected override void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseaddon)
    { }
}