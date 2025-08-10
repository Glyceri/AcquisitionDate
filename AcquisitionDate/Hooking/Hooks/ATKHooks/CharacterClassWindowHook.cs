using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using AcquistionDate.PetNicknames.TranslatorSystem;
using System;
using AcquisitionDate.Database.Enums;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class CharacterClassWindowHook : DateTextHook
{
    const uint CustomDateTextNodeID = 800;
    const uint CustomLabelTextNodeID = 801;

    AtkTextNode* tNode;
    AtkTextNode* tNode2;

    readonly Hook<AddonCharacterClass.Delegates.ReceiveEvent>? ReceiveEventHook;

    uint lastIndex = 0;
    bool lastValid = false;

    public CharacterClassWindowHook(IUserList userList, IDatabase database, ISheets sheets, Configuration configuration) : base(userList, database, sheets, configuration) 
    {
        ReceiveEventHook = PluginHandlers.Hooking.HookFromAddress<AddonCharacterClass.Delegates.ReceiveEvent>((nint)AddonCharacterClass.StaticVirtualTablePointer->ReceiveEvent, ReceiveEventDetour);
    }

    void ReceiveEventDetour(AddonCharacterClass* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData)
    {
        try
        {
            ReceiveEventHook!.Original(thisPtr, eventType, eventParam, atkEvent, atkEventData);            
        }
        catch(Exception e) 
        { 
            PluginHandlers.PluginLog.Error(e, "Calling Origial on the Receive Event hook failed.");
        }

        if (!HandleLastIndex(thisPtr, eventParam)) return;

        HandleTextNode(new BaseNode(&thisPtr->AtkUnitBase), thisPtr);
    }

    void HandleTextNode(BaseNode baseNode, AddonCharacterClass* thisPtr)
    {
        AtkTextNode* baseTextNode = baseNode.GetNode<AtkTextNode>(90);
        if (baseTextNode == null) return;

        tNode = GetTextNode(thisPtr, CustomDateTextNodeID);
        if (tNode == null)
        {
            tNode = CreateTextNode(CustomDateTextNodeID);
            if (tNode == null) return;

            AddSibling(tNode, &baseTextNode->AtkResNode, &thisPtr->UldManager);

            tNode->SetAlignment(AlignmentType.Left);
            tNode->DrawFlags = baseTextNode->DrawFlags;
            tNode->TextFlags = baseTextNode->TextFlags;

            tNode->EdgeColor = baseTextNode->EdgeColor;
            tNode->Color = baseTextNode->Color;
            tNode->TextColor = baseTextNode->TextColor;

            tNode->SetYFloat(baseTextNode->Y + 18);
            tNode->SetXFloat(185);
        }

        tNode2 = GetTextNode(thisPtr, CustomLabelTextNodeID);
        if (tNode2 == null)
        {
            tNode2 = CreateTextNode(CustomLabelTextNodeID);
            if (tNode2 == null) return;

            AddSibling(tNode2, &tNode->AtkResNode, &thisPtr->UldManager);

            tNode2->LineSpacing = 12;
            tNode2->AlignmentFontType = 5;
            tNode2->AlignmentType = AlignmentType.Right;
            tNode2->DrawFlags = 8;
            tNode2->TextFlags = TextFlags.Glare;
            tNode2->SetWidth(165);

            tNode2->EdgeColor = baseTextNode->EdgeColor;
            tNode2->Color = baseTextNode->Color;
            tNode2->TextColor = baseTextNode->TextColor;

            tNode2->SetYFloat(baseTextNode->Y + 18);
            tNode2->SetXFloat(7);
            tNode2->SetText(Translator.GetLine("AchievedOn"));
        }

        PluginHandlers.PluginLog.Verbose($"Level Log Hovered index: {lastIndex}");
        lastValid = DrawDate(tNode, lastIndex, showAlt: false, stillDraw: false);
        tNode2->ToggleVisibility(lastValid);
    }

    AtkTextNode* GetTextNode(AddonCharacterClass* thisPtr, uint id)
    {
        for (int i = 0; i < thisPtr->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = thisPtr->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != id) continue;

            return rNode->GetAsAtkTextNode();
        }

        return null;
    }

    bool HandleLastIndex(AddonCharacterClass* thisPtr, int eventParam)
    {
        try
        {
            int eventIndex = eventParam - 2;
            if (eventIndex < 0) return false;
            if (eventIndex >= thisPtr->ClassComponents.Length) return false;

            uint classLevel = thisPtr->ClassEntries[eventIndex].Level;

            AtkComponentBase* atkBase = thisPtr->ClassComponents[eventIndex].Value;
            if (atkBase == null) return false;

            AtkResNode* resNode = atkBase->AtkResNode;
            if (resNode == null) return false;

            AtkResNode* parentNode = resNode->ParentNode;
            if (parentNode == null) return false;

            AtkComponentNode* componentNode = parentNode->GetAsAtkComponentNode();
            if (componentNode == null) return false;

            AtkComponentBase* componentBase = componentNode->Component;
            if (componentBase == null) return false;

            AtkTextNode* atkTextNode = componentBase->GetTextNodeById(3);
            if (atkTextNode == null) return false;

            string classJobName = atkTextNode->NodeText.ToString();
            classJobName = classJobName.Trim();

            ClassJob? outcome = null;

            foreach (ClassJob cJob in Sheets.AllClassJobs)
            {
                if (!string.Equals(cJob.Name.ExtractText(), classJobName, StringComparison.InvariantCultureIgnoreCase)) continue;

                outcome = cJob;
                break;
            }

            sbyte expIndex = outcome?.ExpArrayIndex ?? 0;
            uint indexBase = (uint)expIndex * 10000;
            indexBase += classLevel;

            lastIndex = indexBase;
            PluginHandlers.PluginLog.Verbose($"Found hovered class to be: {lastIndex}");
            return true;
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Issue in reading class.");
            return false;
        } 
    }

    public override void Init()
    {
        ReceiveEventHook?.Enable();

        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Character", CharacterHookDetour);
    }

    void CharacterHookDetour(AddonEvent type, AddonArgs args)
    {
        AddonCharacter* addon = (AddonCharacter*)args.Addon.Address;

        BaseNode baseNode = new BaseNode(&addon->AtkUnitBase);
        
        ComponentNode componentNode = baseNode.GetComponentNode(82);
        if (componentNode == null) return;

        AtkComponentNode* atkComponentNode = componentNode.GetPointer();
        if (atkComponentNode == null) return;

        if (addon->TabIndex == 2 && lastValid)
        {
            atkComponentNode->SetScaleY(1.02f);
        }
        else
        {
            atkComponentNode->SetScaleY(1f);
        }
    }

    protected override IDatableList GetList(IDatableData userData) => userData.GetDate(AcquirableDateType.ClassLVL);
    protected override bool HandleConfig(Configuration configuration) => configuration.DrawDatesOnLevelScreen;

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon) { }

    protected override void OnDispose()
    {
        ReceiveEventHook?.Dispose();

        PluginHandlers.AddonLifecycle.UnregisterListener(CharacterHookDetour);

        TrySafeInvalidateUIElement(ref tNode);
        TrySafeInvalidateUIElement(ref tNode2);
    }
}
