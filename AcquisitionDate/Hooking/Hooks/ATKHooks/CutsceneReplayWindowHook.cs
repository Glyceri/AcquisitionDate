using Acquisition.PetNicknames.Hooking;
using AcquisitionDate;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

internal unsafe class CutsceneReplayWindowHook : DateTextHook
{
    const uint customJournalTextNodeID = 80;

    AtkTextNode* tNode;

    uint lastQuestID = 0;

    bool isQuest = false;

    public CutsceneReplayWindowHook(IUserList userList, IDatabase database, ISheets sheets, Configuration configuration) : base(userList, database, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "CutSceneReplay", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData)
    {
        if (isQuest) return userData.GetDate(AcquirableDateType.Quest);
        return userData.GetDate(AcquirableDateType.Duty);
    }

    protected override bool HandleConfig(Configuration configuration) => configuration.DrawDatesOnCutsceneReplay;

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

        tNode = AccurateGrabTextNode(cutsceneReplayDetail, customJournalTextNodeID);
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

        string contentName = siblingNode->NodeText.ToString().Trim();

        Quest? quest = Sheets.GetQuest(contentName);

        uint questID = quest?.RowId ?? 0;

        if (questID != 0)
        {
            isQuest = true;
        }
        else
        {
            isQuest = false;
            ContentFinderCondition? contentFinder = Sheets.GetContentFinderConditionByName(contentName);
            questID = contentFinder?.RowId ?? 0;
        }

        if (lastQuestID == questID) return;

        lastQuestID = questID;

        PluginHandlers.PluginLog.Verbose($"Clicked on quest or content: {questID}");

        if (DrawDate(tNode, questID, showAlt: false, stillDraw: true))
        {
            GiveTooltip(cutsceneReplayDetail, tNode, questID, isQuest);
            cutsceneReplayDetail->SetX((short)(cutsceneReplayDetail->X + 1)); // This forces an update and only THEN does the text become visible :/ (seems to produce no side effects currently)
        }
        else
        {
            ClearOldTooldtips();
        }
    }
}