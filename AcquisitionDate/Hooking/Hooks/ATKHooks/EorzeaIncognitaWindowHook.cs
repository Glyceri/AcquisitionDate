using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Nodes;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class EorzeaIncognitaWindowHook : DateTextHook
{
    private EorzeaIncognitaResNode? EorzeaIncognitaResNode;
    
    public EorzeaIncognitaWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "AdventureNoteBook", OnRequestedUpdate);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,          "AdventureNoteBook", OnSetup);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize,        "AdventureNoteBook", OnFinalize);
    }

    protected override void OnDispose() 
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnRequestedUpdate);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnFinalize);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnSetup);
    }
    
    private void OnSetup(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        EorzeaIncognitaResNode = new EorzeaIncognitaResNode();
        EorzeaIncognitaResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void OnFinalize(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        EorzeaIncognitaResNode?.Dispose();
        EorzeaIncognitaResNode = null;
    }
    
    private void OnRequestedUpdate(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (EorzeaIncognitaResNode == null)
        {
            return;
        }
        
        // This is how you get the selected index in the log window
        uint selectedIndex = (uint)AtkStage.Instance()->GetNumberArrayData(NumberArrayType.AdventureNoteBook)->IntArray[0]; 
        bool draw          = DrawDateString(EorzeaIncognitaResNode.DateTextNode, DrawTextFlags.DisallowQuestionmarks, AcquirableDateType.Incognita, selectedIndex);
        
        EorzeaIncognitaResNode.IsVisible = draw;
    }
    
    protected override bool HandleConfig(Configuration configuration) 
        => configuration.DrawDatesOnEorzeaIncognita;
    
    
    // TODO: REMOVE
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Incognita);
    
    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
        { }
}
