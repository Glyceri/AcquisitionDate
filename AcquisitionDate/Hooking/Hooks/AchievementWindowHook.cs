using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe class AchievementWindowHook : HookableElement
{
    readonly uint[] listRenderers = [4, 41001, 41002, 41003, 41004, 41005, 41006, 41007, 41008];
    readonly string[] achievementNames = new string[9]; // the 9 is there because list renderers has 9 entries c: c: c: c: c:
    const uint customDateTextNodeID = 80;

    readonly IAcquisitionServices Services;
    readonly IUserList UserList;

    public AchievementWindowHook(IAcquisitionServices services, IUserList userList)
    {
        Services = services;
        UserList = userList;
    }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "Achievement", AchievementDetour);
    }

    void AchievementDetour(AddonEvent type, AddonArgs args)
    {
        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
        if (!addon->IsVisible) return;

        BaseNode baseNode = new BaseNode(addon);

        ComponentNode cNode = baseNode.GetComponentNode(39);
        if (cNode == null) return;
        for (int i = 0; i < listRenderers.Length; i++)
        {
            uint listRendererID = listRenderers[i];

            ComponentNode listRendererNode = cNode.GetComponentNode(listRendererID);
            if (listRendererNode == null) continue;

            AtkImageNode* prevNode = listRendererNode.GetNode<AtkImageNode>(7);
            if (prevNode == null) continue;

            AtkTextNode* titleNode = listRendererNode.GetNode<AtkTextNode>(8);
            if (titleNode == null) continue;

            AtkTextNode* tNode = listRendererNode.GetNode<AtkTextNode>(customDateTextNodeID);
            if (tNode == null) CreateTextNode(ref tNode, &prevNode->AtkResNode, titleNode, listRendererNode);
            if (tNode == null) continue;

            string achievementName = titleNode->NodeText.ToString();
            if (achievementNames[i] == achievementName) continue;

            achievementNames[i] = achievementName;

            Achievement? achievement = Services.Sheets.GetAchievement(achievementName);
            if (achievement == null)
            {
                tNode->SetText(string.Empty); 
                continue;
            }

            IDatableUser? localUser = UserList.ActiveUser;
            if (localUser == null) // In reality it cannot be null whilst this code runs. But... you know
            {
                tNode->SetText(string.Empty);
                continue;
            }

            DateTime? achievementDate = localUser.Data.AchievementList.GetDate(achievement.Value.RowId);
            if (achievementDate == null)
            {
                tNode->SetText(string.Empty);
                continue;
            }

            tNode->TextColor.R = titleNode->TextColor.R;
            tNode->TextColor.G = titleNode->TextColor.G;
            tNode->TextColor.B = titleNode->TextColor.B;
            tNode->TextColor.A = titleNode->TextColor.A;

            tNode->SetText(achievementDate.Value.ToString("dd/MM/yyyy"));
        }
    }

    void CreateTextNode(ref AtkTextNode* tNode, AtkResNode* prevNode, AtkTextNode* nextNode, ComponentNode listRendererNode)
    {
        tNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (tNode == null) return;

        tNode->AtkResNode.Type = NodeType.Text;
        tNode->AtkResNode.NodeId = customDateTextNodeID;

        tNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorBottom;
        tNode->AtkResNode.DrawFlags = 0;

        tNode->LineSpacing = 18;
        tNode->AlignmentFontType = 0x00;
        tNode->FontSize = 10;
        tNode->TextFlags = (byte)(TextFlags.AutoAdjustNodeSize);
        tNode->TextFlags2 = 0;

        tNode->AtkResNode.SetXFloat(nextNode->GetXFloat());
        tNode->AtkResNode.Y += 3;

        AtkResNode* nextSibling = prevNode->NextSiblingNode;
        tNode->AtkResNode.ParentNode = prevNode->ParentNode;
        tNode->PrevSiblingNode = prevNode;
        tNode->NextSiblingNode = &nextNode->AtkResNode;

        prevNode->NextSiblingNode = &tNode->AtkResNode;
        nextNode->PrevSiblingNode = &tNode->AtkResNode;

        tNode->SetText("--/--/----");
        listRendererNode.GetPointer()->Component->UldManager.UpdateDrawNodeList();
    }

    public override void Dispose()
    {
        PluginHandlers.AddonLifecycle.UnregisterListener(AchievementDetour);
    }
}
