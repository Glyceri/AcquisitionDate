using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using Lumina.Excel.Sheets;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Hooking.Structs;
using AcquisitionDate.Nodes;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class OrnamentWindowHook : DateTextHook
{
    private FasionSelectDateResNode? _ornamentSelectDateResNode;
    
    public OrnamentWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize,      "OrnamentNoteBook", OnFinalize);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "OrnamentNoteBook", OnReceiveEvent);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,        "OrnamentNoteBook", OnSetup);
    }

    protected override void OnDispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnFinalize);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnReceiveEvent);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnSetup);
    }
    
    protected override bool HandleConfig(Configuration configuration) 
        => configuration.DrawDatesOnGlassesSelect;
    
    private void OnSetup(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        _ornamentSelectDateResNode = new FasionSelectDateResNode();
        _ornamentSelectDateResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void OnFinalize(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        _ornamentSelectDateResNode?.Dispose();
        _ornamentSelectDateResNode = null;
    }
    
    private void OnReceiveEvent(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (_ornamentSelectDateResNode == null)
        {
            return;
        }
        
        _ornamentSelectDateResNode.IsVisible = false;
        
        MyAddonOrnamentBook*   myAddonOrnamentNoteBook = (MyAddonOrnamentBook*)addonArgs.Addon.Address;
        AddonOrnamentNoteBook* addonOrnamentNoteBook   = (AddonOrnamentNoteBook*)addonArgs.Addon.Address;
        
        if (myAddonOrnamentNoteBook == null)
        {
            return;
        }
        
        ushort selectedFashionIndex = myAddonOrnamentNoteBook->SelectedFashionIndex;
        
        selectedFashionIndex *= 27; // To get the array index
        selectedFashionIndex += 13; // The array start (well, icon array start)
        
        if (selectedFashionIndex >= addonOrnamentNoteBook->AtkValuesCount)
        {
            return;
        }
        
        uint      fashionIconId = addonOrnamentNoteBook->AtkValues[selectedFashionIndex].UInt;
        Ornament? ornament      = Services.Sheets.GetOrnamentByIconId(fashionIconId);
        
        if (ornament == null)
        {
            return;
        }

        bool draw = DrawDateString(_ornamentSelectDateResNode.DateTextNode, DrawTextFlags.None, AcquirableDateType.Fashion, ornament.Value.RowId);
        
        _ornamentSelectDateResNode.IsVisible = draw;
    }
    
    // TODO: REMOVE VVV
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Facewear);

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
        { }
}
