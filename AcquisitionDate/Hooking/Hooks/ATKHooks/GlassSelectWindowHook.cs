using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class GlassSelectWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    AtkTextNode* tNode;

    public GlassSelectWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "Tooltip", HookDetour);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "GlassSelect", HookDetour);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "GlassSelect", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.FacewearList;

    protected override void OnDispose()
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    bool lastWasGlasses = false;

    IGlassesSheetData? lastGlasses;

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        if (_lastEventType == AddonEvent.PostReceiveEvent && _lastAddonName == "GlassSelect")
        {
            lastWasGlasses = true;
        }

        if (_lastEventType == AddonEvent.PostRefresh && _lastAddonName == "GlassSelect")
        {
            AtkTextNode* titleNode = baseNode.GetNode<AtkTextNode>(22);
            if (titleNode == null) return;

            AtkResNode* siblingNode = baseNode.GetNode<AtkResNode>(23);
            if (siblingNode == null) return;

            tNode = baseNode.GetNode<AtkTextNode>(customDateTextNodeID);
            if (tNode == null)
            {
                tNode = CreateTextNode(customDateTextNodeID);
                if (tNode == null) return;

                MergeTextBetweenElements(tNode, &titleNode->AtkResNode, siblingNode, & baseAddon->UldManager);

                tNode->SetYFloat(269);
                tNode->SetXFloat(189);

                tNode->TextColor = titleNode->TextColor;
                tNode->EdgeColor = titleNode->EdgeColor;
                tNode->BackgroundColor = titleNode->BackgroundColor;
            }

            uint glassesModel = (uint)(lastGlasses?.Model ?? 0);

            if (DrawDate(tNode, glassesModel, true))
            {
                GiveTooltip(baseAddon, tNode, glassesModel);
            }
        }

        if (_lastEventType == AddonEvent.PreRequestedUpdate && _lastAddonName == "Tooltip")
        {
            if (!lastWasGlasses)
            {
                lastGlasses = null;
                return;
            }
            lastWasGlasses = false;

            AtkTextNode* tooltipText = baseNode.GetNode<AtkTextNode>(2);
            if (tooltipText == null) return;

            string tooltipString = tooltipText->NodeText.ToString();

            lastGlasses = Sheets.GetGlassesByName(tooltipString);
            if (lastGlasses == null) return;

            PluginHandlers.PluginLog.Verbose($"Found Glasses via Tooltip: {lastGlasses.BaseSingular}, {lastGlasses.Model}");
        }
    }
}
