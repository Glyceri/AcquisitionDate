using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Nodes;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class GlassSelectWindowHook : DateTextHook
{
    private GlassSelectDateResNode? _glassSelectDateResNode;
    
    public GlassSelectWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services)
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize,      "GlassSelect", OnFinalize);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "GlassSelect", OnReceiveEvent);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,        "GlassSelect", OnSetup);
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
        _glassSelectDateResNode = new GlassSelectDateResNode();
        _glassSelectDateResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void OnFinalize(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        _glassSelectDateResNode?.Dispose();
        _glassSelectDateResNode = null;
    }
    
    private void OnReceiveEvent(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (_glassSelectDateResNode == null)
        {
            return;
        }
        
        _glassSelectDateResNode.IsVisible = false;
        
        AddonGlassSelect* addonGlassSelect = (AddonGlassSelect*)addonArgs.Addon.Address;
        
        if (addonGlassSelect == null)
        {
            return;
        }
        
        ushort index = addonGlassSelect->SelectedGlassesStyleIndex;
        
        index *= 5; // To get the array index
        index += 8; // The array start (well, icon array start)
        
        if (index >= addonGlassSelect->AtkValuesCount)
        {
            return;
        }
        
        uint     glassIconId = addonGlassSelect->AtkValues[index].UInt;
        Glasses? glasses     = Services.Sheets.GetGlassesByIconId(glassIconId);
        
        if (glasses == null)
        {
            return;
        }

        bool draw = DrawDateString(_glassSelectDateResNode.DateTextNode, DrawTextFlags.None, AcquirableDateType.Facewear, glasses.Value.RowId);
        
        _glassSelectDateResNode.IsVisible = draw;
    }
    
    // TODO: REMOVE VVV
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.Facewear);

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
        { }
}
