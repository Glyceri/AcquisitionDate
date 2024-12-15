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

    public QuestJournalWindowHook(IAcquisitionServices services, IUserList userList)
    {
        Services = services;
        UserList = userList;
    }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "Journal", JournalDetour);
    }

    void JournalDetour(AddonEvent type, AddonArgs args)
    {
        if (!AgentQuestJournal.Instance()->IsDisplayingCompletedQuests) return;

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

        if (tNode == null) CreateTextNode(ref tNode, ref siblingNode, ref lvlNode, ref journalDetail);
        if (tNode == null) return;
        tNode->ToggleVisibility(false);

        uint questID = AgentQuestJournal.Instance()->SelectedCompletedQuestId;
        questID += ushort.MaxValue + 1;

        tNode->SetText((questID - ushort.MaxValue).ToString());
        tNode->ToggleVisibility(true);

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return;

        DateTime? questDateTime = localUser.Data.QuestList.GetDate(questID);
        if (questDateTime == null) return;

        tNode->SetText(questDateTime.Value.ToString("dd/MM/yyyy"));
        tNode->ToggleVisibility(true);
    }

    unsafe void CreateTextNode(ref AtkTextNode* tNode, ref AtkTextNode* siblingNode, ref AtkTextNode* levelNode, ref AtkUnitBase* owner)
    {
        tNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (tNode == null) return;

        tNode->AtkResNode.Type = NodeType.Text;
        tNode->AtkResNode.NodeId = customJournalTextNodeID;

        tNode->AtkResNode.NodeFlags = NodeFlags.AnchorTop | NodeFlags.AnchorBottom;
        tNode->AtkResNode.DrawFlags = 0;

        tNode->LineSpacing = 18;
        tNode->AlignmentFontType = 0;
        tNode->FontSize = 12;
        tNode->TextFlags = (byte)(TextFlags.AutoAdjustNodeSize);
        tNode->TextFlags2 = siblingNode->TextFlags2;

        tNode->TextColor.R = siblingNode->TextColor.R;
        tNode->TextColor.G = siblingNode->TextColor.G;
        tNode->TextColor.B = siblingNode->TextColor.B;
        tNode->TextColor.A = siblingNode->TextColor.A;

        tNode->AtkResNode.X = 34;
        tNode->AtkResNode.Y = 100;

        tNode->AtkResNode.ParentNode = siblingNode->ParentNode;
        siblingNode->PrevSiblingNode = &tNode->AtkResNode;
        tNode->NextSiblingNode = &siblingNode->AtkResNode;

        tNode->SetText("--/--/----");
        (&owner->UldManager)->UpdateDrawNodeList();
    }

    public override void Dispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(JournalDetour);
    }
}