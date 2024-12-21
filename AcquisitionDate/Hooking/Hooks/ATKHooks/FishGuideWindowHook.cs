using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class FishGuideWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    AtkTextNode* tNode;

    uint lastID = 0;

    public FishGuideWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) 
    {

    }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "FishGuide2", HookDetour);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "FishGuide2", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.FishingList;
    protected override bool HandleConfig(Configuration configuration) => configuration.DrawDatesOnFishGuide;

    protected override void OnDispose() 
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        if (_lastEventType == AddonEvent.PreReceiveEvent)
        {
            uint fishItemID = (uint)PluginHandlers.GameGui.HoveredItem;
            foreach (FishParameter fish in Sheets.AllFishies)
            {
                if (fish.Item.RowId != fishItemID) continue;

                lastID = fish.RowId;
                break;
            }

            foreach (SpearfishingItem spearfishingItem in Sheets.AllSpearFishies)
            {
                if (spearfishingItem.Item.RowId != fishItemID) continue;

                lastID = spearfishingItem.RowId;
                break;
            }

            PluginHandlers.PluginLog.Verbose($"Received event for fishy: {lastID}");
            return;
        }
        else if (_lastEventType == AddonEvent.PostRequestedUpdate)
        {
            AtkTextNode* titleNode = baseNode.GetNode<AtkTextNode>(136);
            if (titleNode == null) return;

            AtkTextNode* stuffeNode = baseNode.GetNode<AtkTextNode>(137);
            if (stuffeNode == null) return;

            AtkTextNode* sternNode = baseNode.GetNode<AtkTextNode>(138);
            if (sternNode == null) return;

            AtkTextNode* standortNode = baseNode.GetNode<AtkTextNode>(139);
            if (standortNode == null) return;

            tNode = baseNode.GetNode<AtkTextNode>(customDateTextNodeID);
            if (tNode == null)
            {
                tNode = CreateTextNode(customDateTextNodeID);
                if (tNode == null) return;

                MergeTextBetweenElements(tNode, &titleNode->AtkResNode, &stuffeNode->AtkResNode, &baseAddon->UldManager);

                tNode->SetAlignment(AlignmentType.Left);
                tNode->DrawFlags = stuffeNode->DrawFlags;
                tNode->TextFlags = stuffeNode->TextFlags;
                tNode->TextFlags2 = stuffeNode->TextFlags2;

                tNode->EdgeColor = stuffeNode->EdgeColor;
                tNode->Color = stuffeNode->Color;
                tNode->TextColor = stuffeNode->TextColor;

                tNode->SetYFloat(19);
                tNode->SetXFloat(0);

                stuffeNode->SetYFloat(35);
                standortNode->SetYFloat(35);
                sternNode->SetYFloat(35);
            }

            if (lastID == 0)
            {
                tNode->ToggleVisibility(false);
                return;
            }

            if (DrawDate(tNode, lastID, true))
            {
                GiveTooltip(baseAddon, tNode, lastID);
            }
            else
            {
                ClearOldTooldtips();
            }
        }
    }
}
