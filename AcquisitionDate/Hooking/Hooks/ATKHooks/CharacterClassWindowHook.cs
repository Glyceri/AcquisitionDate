using System;
using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Nodes;
using Dalamud.Game.Addon.Events;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class CharacterClassWindowHook : DateTextHook
{
    private CharacterClassResNode? CharacterClassResNode;
    
    public CharacterClassWindowHook(IDatabase database, IAcquisitionServices services)
        : base(database, services) 
        { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "Character",      OnReceiveEventCharacter);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "CharacterClass", OnReceiveEventCharacterClass);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup,        "CharacterClass", SetupCharacterClass);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize,      "CharacterClass", FinalizeCharacterClass);
    }
    
    protected override void OnDispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(OnReceiveEventCharacterClass);
        PluginHandlers.AddonLifecycle.UnregisterListener(OnReceiveEventCharacter);
        PluginHandlers.AddonLifecycle.UnregisterListener(FinalizeCharacterClass);
        PluginHandlers.AddonLifecycle.UnregisterListener(SetupCharacterClass);
    }
    
    protected override bool HandleConfig(Configuration configuration)
        => configuration.DrawDatesOnLevelScreen;
    
    private void SetupCharacterClass(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        CharacterClassResNode = new CharacterClassResNode();
        CharacterClassResNode.AttachNode((AtkUnitBase*)addonArgs.Addon.Address);
    }
    
    private void FinalizeCharacterClass(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        CharacterClassResNode = null;
    }
    
    private void OnReceiveEventCharacter(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        AddonCharacter* addonCharacter = (AddonCharacter*)addonArgs.Addon.Address;
        
        if (addonCharacter == null)
        {
            return;
        }
        
        HandleTab(addonCharacter, addonCharacter->TabIndex == 2);
    }
    
    private void OnReceiveEventCharacterClass(AddonEvent addonEvent, AddonArgs addonArgs)
    {
        if (addonArgs is not AddonReceiveEventArgs eventArgs)
        {
            return;
        }
        
        AddonCharacterClass* addonCharacterClass = (AddonCharacterClass*)eventArgs.Addon.Address;
        
        if (addonCharacterClass == null)
        {
            return;
        }
        
        int classHoverIndex = eventArgs.EventParam - 2;
        
        if (classHoverIndex > 0x21)
        {
            return;
        }
        
        switch (eventArgs.AtkEventType)
        {
            case AddonEventType.MouseOver: HandleMouseIn(addonCharacterClass, classHoverIndex); break;
            case AddonEventType.MouseOut:  HandleMouseOut(); break;
        }
    }
    
    private void HandleMouseIn(AddonCharacterClass* addonCharacterClass, int classIndex)
    {
        if (CharacterClassResNode == null)
        {
            return;
        }

        if (!GetClassJob(addonCharacterClass, classIndex, out uint classJobIndex))
        {
            return;
        }
        
        bool drew = DrawDateString(CharacterClassResNode.DateTextNode.Node, DrawTextFlags.None, AcquirableDateType.ClassLVL, classJobIndex);
        
        CharacterClassResNode.IsVisible = (addonCharacterClass->ClassEntries[classIndex].Level > 0 && drew);
    }

    private bool GetClassJob(AddonCharacterClass* addonCharacterClass, int classIndex, out uint classJobIndex)
    {
        classJobIndex = 0;
        
        uint              classLevel       = addonCharacterClass->ClassEntries[classIndex].Level;
        AtkComponentBase* atkComponentBase = addonCharacterClass->ClassComponents[classIndex];
        
        if (atkComponentBase == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = atkComponentBase->GetTextNodeById(3);
        
        if (textNode == null)
        {
            return false;
        }
        
        string    classJobName = textNode->NodeText.ToString().Trim();
        ClassJob? outcome      = null;

        foreach (ClassJob cJob in Services.Sheets.AllClassJobs)
        {
            if (!string.Equals(cJob.Name.ExtractText(), classJobName, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            outcome = cJob;
            
            break;
        }
        
        if (outcome == null)
        {
            return false;
        }
        
        sbyte expIndex  = outcome.Value.ExpArrayIndex;
        uint  indexBase = (uint)expIndex * 10000;
        
        indexBase += classLevel;
        
        classJobIndex = indexBase;
        
        return true;
    }
    
    private void HandleMouseOut()
    {
        CharacterClassResNode?.IsVisible = false;
    }
    
    private void HandleTab(AddonCharacter* addonCharacter, bool isCharacterTab)
    {
        addonCharacter->WindowNode->SetScaleY(isCharacterTab ? 1.02f : 1.0f);
    }
    
    protected override IDatableList GetList(IDatableData userData) 
        => userData.GetDateList(AcquirableDateType.ClassLVL);

    protected override void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon) { }
}