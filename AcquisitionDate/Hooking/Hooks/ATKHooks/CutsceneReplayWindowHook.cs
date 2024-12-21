using Acquisition.PetNicknames.Hooking;
using AcquisitionDate;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

internal unsafe class CutsceneReplayWindowHook : DateTextHook
{
    const uint customJournalTextNodeID = 80;

    AtkTextNode* tNode;

    uint lastQuestID = 0;

    public CutsceneReplayWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "CutSceneReplay", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.QuestList;

    protected override void OnDispose()
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseaddon)
    {
        AtkUnitBase* cutsceneReplayDetail = AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("AddonCutSceneReplayDetail");
        if (cutsceneReplayDetail == null || !cutsceneReplayDetail->IsVisible) return;

        BaseNode newBaseNode = new BaseNode(cutsceneReplayDetail);

        AtkTextNode* siblingNode = newBaseNode.GetNode<AtkTextNode>(10);
        if (siblingNode == null) return;

        AtkResNode* prevNode = newBaseNode.GetNode<AtkResNode>(7);
        if (prevNode == null) return;

        tNode = null;
        for (int i = 0; i < cutsceneReplayDetail->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = cutsceneReplayDetail->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != customJournalTextNodeID) continue;

            tNode = rNode->GetAsAtkTextNode();
            break;
        }
        if (tNode == null)
        {
            tNode = CreateTextNode(customJournalTextNodeID);
            if (tNode == null) return;

            MergeTextBetweenElements(tNode, prevNode, &siblingNode->AtkResNode, &cutsceneReplayDetail->UldManager);

            baseaddon->UldManager.UpdateDrawNodeList();

            tNode->SetAlignment(AlignmentType.BottomRight);

            tNode->AtkResNode.X = 390;
            tNode->AtkResNode.Y = 103;

            tNode->TextColor.R = 51;
            tNode->TextColor.G = 51;
            tNode->TextColor.B = 51;
        }

        Quest? quest = Sheets.GetQuest(siblingNode->NodeText.ToString().Trim());

        uint questID = quest?.RowId ?? 0;

        if (lastQuestID == questID) return;

        lastQuestID = questID;

        PluginHandlers.PluginLog.Verbose($"Clicked on quest: {questID}");

        DrawDate(tNode, questID);
        GiveTooltip(cutsceneReplayDetail, tNode, questID, true);
        cutsceneReplayDetail->SetX((short)(cutsceneReplayDetail->X + 1)); // This forces an update and only THEN does the text become visible :/ (seems to produce no side effects currently)
    }
}