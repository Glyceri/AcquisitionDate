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

internal unsafe class QuestJournalWindowHook : DateTextHook
{
    const uint customJournalTextNodeID = 80;

    uint lastID = 0;
    bool isQuest = false;

    public QuestJournalWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "Journal", HookDetour);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "ContentsFinder", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData)
    {
        if (isQuest) return userData.QuestList;
        return userData.DutyList;
    }

    protected override void OnDispose() { }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseaddon)
    {
        AtkUnitBase* journalDetail = GrabAddon();
        if (journalDetail == null || !journalDetail->IsVisible) return;

        BaseNode newBaseNode = new BaseNode(journalDetail);

        AtkTextNode* siblingNode = newBaseNode.GetNode<AtkTextNode>(38);
        if (siblingNode == null) return;

        AtkTextNode* tNode = AccurateGrabTextNode(journalDetail, customJournalTextNodeID);
        if (tNode == null)
        {
            tNode = CreateTextNode(customJournalTextNodeID);
            if (tNode == null) return;

            AddSibling(tNode, &siblingNode->AtkResNode, &journalDetail->UldManager);
            baseaddon->UldManager.UpdateDrawNodeList();

            tNode->SetAlignment(AlignmentType.BottomRight);

            tNode->AtkResNode.X = 390;
            tNode->AtkResNode.Y = 103;

            tNode->TextColor.R = 51;
            tNode->TextColor.G = 51;
            tNode->TextColor.B = 51;
        }

        tNode->ToggleVisibility(true);

        string contentString = siblingNode->NodeText.ToString().Trim();

        uint currentID = 0;

        if (_lastAddonName == "Journal")
        {
            isQuest = true;

            if (!AgentQuestJournal.Instance()->IsDisplayingCompletedQuests)
            {
                tNode->ToggleVisibility(false);
                return;
            }

            Quest? quest = Sheets.GetQuest(contentString);
            currentID = quest?.RowId ?? 0;

            if (currentID == 0) // Failsafe :shrug:
            {
                currentID = AgentQuestJournal.Instance()->SelectedCompletedQuestId;
                currentID += ushort.MaxValue + 1;
            }
        }
        else if (_lastAddonName == "ContentsFinder")
        {
            isQuest = false;
            ContentFinderCondition? content = Sheets.GetContentFinderConditionByName(contentString);
            currentID = content?.RowId ?? 0;
        }

        if (lastID == currentID) return;

        lastID = currentID;

        PluginHandlers.PluginLog.Verbose($"Clicked on quest or content: {currentID}");

        DrawDate(tNode, currentID, true);
        journalDetail->SetX((short)(journalDetail->X + 1)); // This forces an update and only THEN does the text become visible :/ (seems to produce no side effects currently)
    }

    AtkUnitBase* GrabAddon()
    {
        // 3 is just some arbitrary numbi
        // 1 because indexes start at 1
        for (int journalDetailIndex = 1; journalDetailIndex < 3; journalDetailIndex++)
        {
            AtkUnitBase* journalDetail = AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("JournalDetail", journalDetailIndex);

            uint parentID = journalDetail->ParentId;
            if (parentID == 0) continue;
            if (_lastId != parentID) continue;

            return journalDetail;
        }

        return null;
    }
}