using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class MountWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    AtkTextNode* tNode;

    public MountWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "MountNoteBook", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.MountList;

    protected override void OnDispose()
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        AtkTextNode* titleNode = baseNode.GetNode<AtkTextNode>(65);
        if (titleNode == null) return;

        AtkTextNode* descriptionNode = baseNode.GetNode<AtkTextNode>(66);
        if (descriptionNode == null) return;

        tNode = baseNode.GetNode<AtkTextNode>(customDateTextNodeID);
        if (tNode == null)
        {
            tNode = CreateTextNode(customDateTextNodeID);
            if (tNode == null) return;

            MergeTextBetweenElements(tNode, &titleNode->AtkResNode, &descriptionNode->AtkResNode, &baseAddon->UldManager);

            tNode->SetYFloat(-27);
            tNode->SetXFloat(309);

            tNode->TextColor = titleNode->TextColor;
            tNode->EdgeColor = titleNode->EdgeColor;
            tNode->BackgroundColor = titleNode->BackgroundColor;
        }

        Mount? mount = Sheets.GetMountByName(titleNode->NodeText.ToString());
        if (mount == null)
        {
            tNode->ToggleVisibility(false);
            return;
        }

        uint mountID = mount.Value.RowId;

        PluginHandlers.PluginLog.Verbose($"Mount Notebook notebook clicked ID: {mountID}");

        GiveTooltip(baseAddon, tNode, mountID);
        DrawDate(tNode, mountID);
    }
}
