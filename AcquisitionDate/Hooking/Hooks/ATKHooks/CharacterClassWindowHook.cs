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
using System;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class CharacterClassWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;
    const uint customLabelTextNodeID = 801;

    readonly Hook<AddonCharacterClass.Delegates.ReceiveEvent>? ReceiveEventHook;

    uint lastIndex = 0;

    public CharacterClassWindowHook(IUserList userList, ISheets sheets) : base(userList, sheets) 
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

        AtkTextNode* tNode = null;
        for (int i = 0; i < thisPtr->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = thisPtr->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != customDateTextNodeID) continue;

            tNode = rNode->GetAsAtkTextNode();
            break;
        }

        if (tNode == null)
        {
            tNode = CreateTextNode(customDateTextNodeID);
            if (tNode == null) return;

            AddSibling(tNode, &baseTextNode->AtkResNode, &thisPtr->UldManager);

            tNode->SetAlignment(AlignmentType.Left);
            tNode->DrawFlags = baseTextNode->DrawFlags;
            tNode->TextFlags = baseTextNode->TextFlags;
            tNode->TextFlags2 = baseTextNode->TextFlags2;

            tNode->EdgeColor = baseTextNode->EdgeColor;
            tNode->Color = baseTextNode->Color;
            tNode->TextColor = baseTextNode->TextColor;

            tNode->SetYFloat(baseTextNode->Y + 18);
            tNode->SetXFloat(185);
        }

        AtkTextNode* tNode2 = null;
        for (int i = 0; i < thisPtr->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = thisPtr->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != customLabelTextNodeID) continue;

            tNode2 = rNode->GetAsAtkTextNode();
            break;
        }

        if (tNode2 == null)
        {
            tNode2 = CreateTextNode(customLabelTextNodeID);
            if (tNode2 == null) return;

            AddSibling(tNode2, &tNode->AtkResNode, &thisPtr->UldManager);

            tNode2->LineSpacing = 12;
            tNode2->AlignmentFontType = 5;
            tNode2->AlignmentType = AlignmentType.Right;
            tNode2->DrawFlags = 8;
            tNode2->TextFlags = 16;
            tNode2->TextFlags2 = 0;
            tNode2->SetWidth(165);

            tNode2->EdgeColor = baseTextNode->EdgeColor;
            tNode2->Color = baseTextNode->Color;
            tNode2->TextColor = baseTextNode->TextColor;

            tNode2->SetYFloat(baseTextNode->Y + 18);
            tNode2->SetXFloat(7);
            tNode2->SetText("Achieved On:");
        }

        PluginHandlers.PluginLog.Verbose($"Level Log Hovered index: {lastIndex}");

        DrawDate(tNode, lastIndex, true);
    }

    bool HandleLastIndex(AddonCharacterClass* thisPtr, int eventParam)
    {
        try
        {
            int eventIndex = eventParam - 2;
            if (eventIndex < 0) return false;
            if (eventIndex >= thisPtr->ButtonNodes.Length) return false;

            uint classLevel = thisPtr->ClassEntries[eventIndex].Level;

            AtkComponentButton* buttonComponent = thisPtr->ButtonNodes[eventIndex].Value;
            if (buttonComponent == null) return false;

            AtkResNode* resNode = buttonComponent->AtkResNode;
            if (resNode == null) return false;

            AtkResNode* parentNode = resNode->ParentNode;
            if (parentNode == null) return false;

            AtkComponentNode* componentNode = parentNode->GetAsAtkComponentNode();
            if (componentNode == null) return false;

            AtkComponentBase* componentBase = componentNode->Component;
            if (componentBase == null) return false;

            AtkResNode* atkTextResNode = componentBase->GetTextNodeById(3);
            if (atkTextResNode == null) return false;

            AtkTextNode* atkTextNode = atkTextResNode->GetAsAtkTextNode();
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

            if (outcome == null) return false;

            sbyte expIndex = outcome.Value.ExpArrayIndex;
            uint indexBase = (uint)expIndex * 10000;
            indexBase += classLevel;

            lastIndex = indexBase;
            PluginHandlers.PluginLog.Verbose($"Found hovered class to be: {outcome.Value.Name.ExtractText()} {lastIndex}");
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

        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Character", CharacterHookDetour);
    }

    void CharacterHookDetour(AddonEvent type, AddonArgs args)
    {
        PluginHandlers.PluginLog.Verbose("Setup");
        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;

        BaseNode baseNode = new BaseNode(addon);

        ComponentNode componentNode = baseNode.GetComponentNode(82);
        if (componentNode == null) return;

        AtkComponentNode* atkComponentNode = componentNode.GetPointer();
        if (atkComponentNode == null) return;

        atkComponentNode->SetScaleY(1.02f);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.ClassLVLList;

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon) { }

    protected override void OnDispose()
    {
        ReceiveEventHook?.Dispose();

        PluginHandlers.AddonLifecycle.UnregisterListener(CharacterHookDetour);
    }
}
