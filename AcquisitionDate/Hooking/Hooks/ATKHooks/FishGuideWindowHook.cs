using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Nodes;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class FishGuideWindowHook : DateTextHook
{
    private FishGuideResNode? ResNode;
    
    public FishGuideWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,           "FishGuide2", OnSetup);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize,         "FishGuide2", OnFinalize);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "FishGuide2", OnPostRequest);
    }

    protected override void OnDispose() 
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnSetup);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnFinalize);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnPostRequest);
    }
    
    protected override bool HandleConfig(Configuration configuration) 
        => configuration.DrawDatesOnFishGuide;
    
    
    private void OnSetup(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        ResNode = new FishGuideResNode();
        ResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void OnFinalize(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        ResNode?.Dispose();
        ResNode = null;
    }
    
    private void OnPostRequest(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (ResNode == null)
        {
            return;
        }
        
        bool draw = DrawDateString(ResNode.DateTextNode, DrawTextFlags.DisallowQuestionmarks, AcquirableDateType.Fishing, GetSelectedId());
        
        ResNode.IsVisible = draw;
    }
    
    private uint GetSelectedId()
    {
        int* array       = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.FishGuide)->IntArray;
        bool isSpearFish = array[2] == 1;
        int  index       = array[8];
        int  itemId      = array[9 + (index * 4)];
        
        if (!isSpearFish)
        {
            foreach (FishParameter fish in Services.Sheets.AllFishies)
            {
                if (fish.Item.RowId != itemId)
                {
                    continue;
                }
                
                return fish.RowId;
            }
        }
        else
        {
            foreach (SpearfishingItem spearfishingItem in Services.Sheets.AllSpearFishies)
            {
                if (spearfishingItem.Item.RowId != itemId)
                {
                    continue;
                }

                return spearfishingItem.RowId;
            }
        }
        
        return 0;
    }
    
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Fishing);
    
    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
        { }
}
