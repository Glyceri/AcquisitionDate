using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using AcquiryDate.PetNicknames.Services.ServiceWrappers.Interfaces;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class MinionWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    AtkTextNode* tNode;

    public MinionWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "MinionNoteBook", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.MinionList;

    protected override void OnDispose()
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        AtkTextNode* titleNode = baseNode.GetNode<AtkTextNode>(67);
        if (titleNode == null) return;

        AtkTextNode* descriptionNode = baseNode.GetNode<AtkTextNode>(68);
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

        uint minionID = 0;

        // Instead of reading text I use hover ID because Pet Nicknames LMAOOOO
        PluginHandlers.PluginLog.Verbose($"Clicked minion: {(ushort)PluginHandlers.GameGui.HoveredAction.ActionID}");
        IPetSheetData? acquiredPet = Sheets.GetCompanion((ushort)PluginHandlers.GameGui.HoveredAction.ActionID);
        
        if (acquiredPet != null)
        {
            minionID = (uint)acquiredPet.ID;
        }

        PluginHandlers.PluginLog.Verbose($"Minion Notebook notebook clicked ID: {minionID}");

        GiveTooltip(baseAddon, tNode, minionID);
        DrawDate(tNode, minionID, true);
    }
}
