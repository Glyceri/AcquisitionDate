using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Acquisition.PetNicknames.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Core.Handlers;
using System;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe abstract class DateTextHook : HookableElement
{
    protected readonly IUserList UserList;
    protected readonly ISheets Sheets;

    public DateTextHook(IUserList userList, ISheets sheets)
    {
        UserList = userList;
        Sheets = sheets;
    }

    protected void HookDetour(AddonEvent type, AddonArgs args)
    {
        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
        if (!addon->IsVisible) return;

        BaseNode baseNode = new BaseNode(addon);

        OnHookDetour(baseNode, ref addon);
    }

    protected abstract void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon);

    protected AtkTextNode* CreateTextNode(uint nodeID)
    {
        AtkTextNode* tNode = IMemorySpace.GetUISpace()->Create<AtkTextNode>();
        if (tNode == null)
        {
            PluginHandlers.PluginLog.Fatal("Tried to create an ATKTextNode but failed. This should be fatal.");
            return null;
        }

        tNode->AtkResNode.Type = NodeType.Text;
        tNode->AtkResNode.NodeId = nodeID;

        tNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorBottom;
        tNode->AtkResNode.DrawFlags = 0;
        tNode->SetAlignment(AlignmentType.BottomLeft);
        tNode->TextFlags2 = 0;

        tNode->LineSpacing = 18;
        tNode->FontSize = 12;
        tNode->AlignmentFontType = 0;
        tNode->TextFlags = (byte)(TextFlags.AutoAdjustNodeSize);

        tNode->TextColor.R = 0;
        tNode->TextColor.G = 0;
        tNode->TextColor.B = 0;
        tNode->TextColor.A = 255;

        tNode->SetText("--/--/----");
        return tNode;
    }

    protected void DrawDate(AtkTextNode* textNode, uint listID, bool stillDraw = false)
    {
        if (textNode == null) return;

        textNode->ToggleVisibility(stillDraw);
        textNode->SetText("--/--/----");

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return;

        IDatableList list = GetList(localUser.Data);

        DateTime? dateTime = list.GetDate(listID);
        if (dateTime == null) return;

        textNode->ToggleVisibility(true);
        textNode->SetText(dateTime.Value.ToString("dd/MM/yyyy").Replace("-", "/"));
    }

    protected abstract IDatableList GetList(IDatableData userData);

    protected void MergeTextBetweenElements(AtkTextNode* baseNode, AtkResNode* prevNode, AtkResNode* nextNode, AtkUldManager* uldManager)
    {
        baseNode->AtkResNode.ParentNode = prevNode->ParentNode;

        AtkResNode* nextSibling = prevNode->NextSiblingNode;
        baseNode->AtkResNode.ParentNode = prevNode->ParentNode;
        baseNode->PrevSiblingNode = prevNode;
        baseNode->NextSiblingNode = nextNode;

        prevNode->NextSiblingNode = &baseNode->AtkResNode;
        nextNode->PrevSiblingNode = &baseNode->AtkResNode;

        uldManager->UpdateDrawNodeList();
    }

    protected void AddSibling(AtkTextNode* baseNode, AtkResNode* prevNode, AtkUldManager* uldManager)
    {
        baseNode->AtkResNode.ParentNode = prevNode->ParentNode;

        prevNode->PrevSiblingNode = &baseNode->AtkResNode;
        baseNode->NextSiblingNode = prevNode;

        uldManager->UpdateDrawNodeList();
    }

    public sealed override void Dispose()
    {
        try
        {
            PluginHandlers.AddonLifecycle.UnregisterListener(HookDetour);
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Hook failed to dispose.");
        }
        try
        {
            OnDispose();
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Hook failed to dispose.");
        }
    }

    protected abstract void OnDispose();
}
