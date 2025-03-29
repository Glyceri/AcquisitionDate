using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using Lumina.Excel.Sheets;
using AcquisitionDate.Database.Enums;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class OrnamentWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    AtkTextNode* tNode;

    public OrnamentWindowHook(IUserList userList, IDatabase database, ISheets sheets, Configuration configuration) : base(userList, database, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "OrnamentNoteBook", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.GetDate(AcquirableDateType.Fashion);
    protected override bool HandleConfig(Configuration configuration) => configuration.DrawDatesOnFashionSelect;

    protected override void OnDispose()
    {
        TrySafeInvalidateUIElement(ref tNode);
    }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        AtkTextNode* titleNode = baseNode.GetNode<AtkTextNode>(60);
        if (titleNode == null) return;

        tNode = baseNode.GetNode<AtkTextNode>(customDateTextNodeID);
        if (tNode == null)
        {
            tNode = CreateTextNode(customDateTextNodeID);
            if (tNode == null) return;

            AddSibling(tNode, &titleNode->AtkResNode, &baseAddon->UldManager);

            tNode->SetYFloat(-37);
            tNode->SetXFloat(210);

            tNode->TextColor = titleNode->TextColor;
            tNode->EdgeColor = titleNode->EdgeColor;
            tNode->BackgroundColor = titleNode->BackgroundColor;
        }

        Ornament? ornament = Sheets.GetOrnamentByName(titleNode->NodeText.ToString());
        if (ornament == null)
        {
            tNode->ToggleVisibility(false);
            return;
        }

        uint ornamentID = ornament.Value.RowId;

        PluginHandlers.PluginLog.Verbose($"Ornament Notebook clicked ID: {ornamentID}");

        if (DrawDate(tNode, ornamentID, showAlt: false, stillDraw: true))
        {
            GiveTooltip(baseAddon, tNode, ornamentID);
        }
    }
}
