using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Hooking;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

internal unsafe class QuestJournalWindowHook : HookableElement
{
    const uint customJournalTextNodeID = 80;

    readonly IAcquisitionServices Services;
    readonly IUserList UserList;

    uint lastQuestID = 0;

    public QuestJournalWindowHook(IAcquisitionServices services, IUserList userList)
    {
        Services = services;
        UserList = userList;
    }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "Journal", JournalDetour);
    }

    void JournalDetour(AddonEvent type, AddonArgs args)
    {
        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
        if (!addon->IsVisible) return;

        AtkUnitBase* journalDetail = AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("JournalDetail");
        if (journalDetail == null || !journalDetail->IsVisible) return;

        BaseNode baseNode = new BaseNode(journalDetail);

        AtkTextNode* siblingNode = baseNode.GetNode<AtkTextNode>(38);
        if (siblingNode == null) return;

        AtkTextNode* lvlNode = baseNode.GetNode<AtkTextNode>(9);
        if (lvlNode == null) return;

        AtkTextNode* tNode = null;

        for (int i = 0; i < journalDetail->UldManager.NodeListCount; i++)
        {
           AtkResNode* rNode = journalDetail->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode -> NodeId != customJournalTextNodeID) continue;

            tNode = rNode->GetAsAtkTextNode();
            break;
        }

        if (tNode == null) CreateTextNode(ref tNode, ref siblingNode, ref lvlNode, ref journalDetail, ref addon);
        if (tNode == null) return;
        tNode->ToggleVisibility(false);

        if (!AgentQuestJournal.Instance()->IsDisplayingCompletedQuests) return;

        uint questID = AgentQuestJournal.Instance()->SelectedCompletedQuestId;
        questID += ushort.MaxValue + 1;

        if (lastQuestID == questID) return;

        lastQuestID = questID;

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return;

        DateTime? questDateTime = localUser.Data.QuestList.GetDate(questID);
        if (questDateTime == null) return;

        tNode->ToggleVisibility(true);
        tNode->NodeText.SetString(questDateTime.Value.ToString("dd/MM/yyyy"));
        journalDetail->SetX((short)(journalDetail->X + 1)); // This forces an update and only THEN does the text become visible :/ (seems to produce no side effects currently)
    }

    unsafe void CreateTextNode(ref AtkTextNode* tNode, ref AtkTextNode* siblingNode, ref AtkTextNode* levelNode, ref AtkUnitBase* owner, ref AtkUnitBase* journal)
    {
        tNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (tNode == null) return;

        tNode->AtkResNode.Type = NodeType.Text;
        tNode->AtkResNode.NodeId = customJournalTextNodeID;

        tNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorBottom;
        tNode->AtkResNode.DrawFlags = 0;
        tNode->SetAlignment(AlignmentType.BottomRight);

        tNode->LineSpacing = 18;
        tNode->FontSize = 12;
        tNode->AlignmentFontType = 0;
        tNode->TextFlags = (byte)(TextFlags.AutoAdjustNodeSize);
        tNode->TextFlags2 = siblingNode->TextFlags2;

        tNode->AtkResNode.X = 390;
        tNode->AtkResNode.Y = 103;

        tNode->TextColor.R = siblingNode->TextColor.R;
        tNode->TextColor.G = siblingNode->TextColor.G;
        tNode->TextColor.B = siblingNode->TextColor.B;
        tNode->TextColor.A = siblingNode->TextColor.A;

        tNode->AtkResNode.ParentNode = siblingNode->ParentNode;
        siblingNode->PrevSiblingNode = &tNode->AtkResNode;
        tNode->NextSiblingNode = &siblingNode->AtkResNode;

        tNode->SetText("--/--/----");
        owner->UldManager.UpdateDrawNodeList();
        journal->UldManager.UpdateDrawNodeList();
    }

    public override void Dispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(JournalDetour);
    }
}