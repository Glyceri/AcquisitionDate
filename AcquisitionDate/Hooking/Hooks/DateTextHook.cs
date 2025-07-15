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
using Dalamud.Game.Addon.Events;
using Dalamud.Utility;
using AcquistionDate.PetNicknames.TranslatorSystem;

namespace AcquisitionDate.Hooking.Hooks;

internal unsafe abstract class DateTextHook : HookableElement
{
    protected readonly Configuration Configuration;
    protected readonly IUserList UserList;
    protected readonly ISheets Sheets;
    protected readonly IDatabase Database;

    protected AddonEvent _lastEventType;
    protected string _lastAddonName = string.Empty;
    protected uint _lastId = 0;

    // TODO: this is a temporary fix to push the update out.
    // Make tooltips work properly again :/
    string? lastDateTimeString = null;

    public DateTextHook(IUserList userList, IDatabase database, ISheets sheets, Configuration configuration)
    {
        Configuration = configuration;
        UserList = userList;
        Sheets = sheets;
        Database = database;
    }

    protected void HookDetour(AddonEvent type, AddonArgs args)
    {
        _lastEventType = type;
        _lastAddonName = args.AddonName;

        AtkUnitBase* addon = (AtkUnitBase*)args.Addon;
        _lastId = addon->Id;
        if (!addon->IsVisible) return;

        BaseNode baseNode = new BaseNode(addon);

        OnHookDetour(baseNode, ref addon);
    }

    protected abstract void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon);
    protected abstract IDatableList GetList(IDatableData userData);
    protected abstract bool HandleConfig(Configuration config);

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

    protected AtkTextNode* AccurateGrabTextNode(AtkUnitBase* unitBase, uint ID)
    {
        for (int i = 0; i < unitBase->UldManager.NodeListCount; i++)
        {
            AtkResNode* rNode = unitBase->UldManager.NodeList[i];
            if (rNode == null) continue;
            if (rNode->NodeId != ID) continue;

            return rNode->GetAsAtkTextNode();
        }

        return null;
    }

    protected bool DrawDate(AtkTextNode* textNode, uint listID, bool showAlt, bool stillDraw = false)
    {
        if (textNode == null) return false;

        textNode->ToggleVisibility(false);
        textNode->SetText(string.Empty);

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null) return false;            // Should really be possible that this is false whilst the hooks still update but who knows

        bool configSaysVisible = HandleConfig(Configuration);
        if (!configSaysVisible) return false;

        string? dateTimeString = Database.GetDateTimeString(listID, GetList, showAlt, localUser.Data);

        // Temporary fix until I fully flesh out this update
        // Fucking PVP tournament taking all my time
        // My team sucks ass
        // I suck ass
        // Why did I join LLLLLLLLLLLLL
        lastDateTimeString = dateTimeString;

        if (dateTimeString.IsNullOrWhitespace())
        {
            return stillDraw;
        }

        textNode->ToggleVisibility(true);
        textNode->SetText(dateTimeString);

        return true;
    }    

    IAddonEventHandle? lastHoverOverEvent;
    IAddonEventHandle? lastHoverOutEvent;

    protected void ClearOldTooldtips()
    {
        if (lastHoverOutEvent != null) PluginHandlers.EventManager.RemoveEvent(lastHoverOutEvent);
        if (lastHoverOverEvent != null) PluginHandlers.EventManager.RemoveEvent(lastHoverOverEvent);
    }

    protected void GiveTooltip(AtkUnitBase* baseAddon, AtkTextNode* baseNode, uint ID, bool hasInaccuracies = false)
    {
        ClearOldTooldtips();

        baseNode->NodeFlags |= NodeFlags.EmitsEvents | NodeFlags.RespondToMouse | NodeFlags.HasCollision;
        lastHoverOverEvent = PluginHandlers.EventManager.AddEvent((nint)baseAddon, (nint)baseNode, AddonEventType.MouseOver, (AddonEventType atkEventType, AddonEventData data) => OnTooltip(atkEventType, data.AddonPointer, (nint)((AtkUnitBase*)data.AddonPointer)->RootNode, ID, hasInaccuracies));
        lastHoverOutEvent  = PluginHandlers.EventManager.AddEvent((nint)baseAddon, (nint)baseNode, AddonEventType.MouseOut, (AddonEventType atkEventType, AddonEventData data) => OnTooltip(atkEventType, data.AddonPointer, (nint)((AtkUnitBase*)data.AddonPointer)->RootNode, ID, hasInaccuracies));
        baseAddon->UpdateCollisionNodeList(false);
    }

    void OnTooltip(AddonEventType atkEventType, nint atkUnitBase, nint atkResNode, uint ID, bool hasInaccuracies)
    {
        if (atkEventType == AddonEventType.MouseOver)
        {
            // Last date time string is TEMPORARY
            // This HAS to be fixed bettr some day
            // Because rn itll go:
            // Achieved On: Before: 00/00/0000
            // or Achieved On: ??/??/????
            // Like hello...
            // This sucks
            string? dateString = lastDateTimeString;

            string newLine = string.Empty;
            if (dateString.IsNullOrWhitespace())
            {
                newLine = Translator.GetLine("NoDate");
            }
            else
            {
                newLine = $"{Translator.GetLine("AchievedOn")} {dateString}";
                if (hasInaccuracies)
                {
                    newLine += $"\n{Translator.GetLine("NotAccurate")}";
                }
            }

            AtkStage.Instance()->TooltipManager.ShowTooltip((ushort)((AtkResNode*)atkResNode)->ParentNode->NodeId, (AtkResNode*)atkResNode, newLine);
        }
        else if (atkEventType == AddonEventType.MouseOut)
        {
            AtkStage.Instance()->TooltipManager.HideTooltip((ushort)((AtkResNode*)atkResNode)->ParentNode->NodeId);
        }
    }

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
        ClearOldTooldtips();

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


    protected void TrySafeInvalidateUIElement(ref AtkTextNode* tNode)
    {
        try
        {
            if (tNode == null) return;
            tNode->ToggleVisibility(false);
            tNode = null;
        }
        catch (Exception e)
        {
            PluginHandlers.PluginLog.Error(e, "Error during safe node cleanup");
        }
    }
}
