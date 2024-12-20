using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking.Hooks;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal unsafe class QuestJournalWindowHook : DateTextHook
{
    const uint customJournalTextNodeID = 80;

    AtkTextNode* tNode;

    uint lastQuestID = 0;

    public QuestJournalWindowHook(IUserList userList, ISheets sheets) : base(userList, sheets) {}

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "Journal", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.QuestList;

    protected override void OnDispose() 
    {
        TrySafeInvalidateUIElement(ref tNode);
    } 

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseaddon)
    {
        AtkUnitBase* journalDetail = AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("JournalDetail");
        if (journalDetail == null || !journalDetail->IsVisible) return;

        BaseNode newBaseNode = new BaseNode(journalDetail);

        AtkTextNode* siblingNode = newBaseNode.GetNode<AtkTextNode>(38);
        if (siblingNode == null) return;

        tNode = null;
        for (int i = 0; i < journalDetail->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = journalDetail->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != customJournalTextNodeID) continue;

            tNode = rNode->GetAsAtkTextNode();
            break;
        }

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

        tNode->ToggleVisibility(false);

        if (!AgentQuestJournal.Instance()->IsDisplayingCompletedQuests)
        {
            lastQuestID = 0;
            return;
        }

        uint questID = AgentQuestJournal.Instance()->SelectedCompletedQuestId;
        questID += ushort.MaxValue + 1;

        PluginHandlers.PluginLog.Verbose($"Clicked on quest: {questID}");

        if (lastQuestID == questID) return;

        lastQuestID = questID;

        DrawDate(tNode, questID);
        GiveTooltip(journalDetail, tNode, questID, true);
        journalDetail->SetX((short)(journalDetail->X + 1)); // This forces an update and only THEN does the text become visible :/ (seems to produce no side effects currently)
    }
}