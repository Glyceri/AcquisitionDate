using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class EorzeaIncognitaWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 80;

    AtkTextNode* tNode;

    public EorzeaIncognitaWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "AdventureNoteBook", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.SightList;

    protected override void OnDispose() 
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        uint selectedIndex = (uint)AtkStage.Instance()->GetNumberArrayData(NumberArrayType.AdventureNoteBook)->IntArray[0]; // This is how you get the selected index in the log window

        AtkTextNode* siblingNode = baseNode.GetNode<AtkTextNode>(59);
        if (siblingNode == null) return;

        AtkImageNode* nextNode = baseNode.GetNode<AtkImageNode>(60);
        if (nextNode == null) return;

        tNode = baseNode.GetNode<AtkTextNode>(customDateTextNodeID);
        if (tNode == null)
        {
            tNode = CreateTextNode(customDateTextNodeID);
            if (tNode == null) return;

            MergeTextBetweenElements(tNode, &siblingNode->AtkResNode, &nextNode->AtkResNode, &baseAddon->UldManager);

            tNode->SetYFloat(siblingNode->Y);
            tNode->SetXFloat(0);

            tNode->TextColor.R = 204;
            tNode->TextColor.G = 204;
            tNode->TextColor.B = 204;
        }

        PluginHandlers.PluginLog.Verbose($"Adventure notebook clicked index: {selectedIndex}");

        GiveTooltip(baseAddon, tNode, selectedIndex);
        DrawDate(tNode, selectedIndex, true);
    }
}
