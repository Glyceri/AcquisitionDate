using AcquisitionDate.Core.Handlers;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Database.Interfaces;
using Acquisition.PetNicknames.Hooking;
using System.Runtime.InteropServices;
using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class OrchestrionWindowHook : DateTextHook
{
    const uint customDateTextNodeID = 800;

    uint[] idArray = [
        3,
        31001,
        31002,
        31003,
        31004,
        31005,
        31006,
        31007,
        31008,
        31009,
        31010,
        31011,
        31012,
        31013,
        31014,
        310015,
        31016,
        31017,
        31018,
        31019,
        ];
    uint[] lastOrchestrionDate = new uint[20];

    public OrchestrionWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init()
    {
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "Orchestrion", HookDetour);
        PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "OrchestrionINN", HookDetour);
    }

    protected override IDatableList GetList(IDatableData userData) => userData.OrchestrionList;
    protected override bool HandleConfig(Configuration configuration) => configuration.DrawDatesOnOrchestrion;

    protected override void OnDispose() { }

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
        ComponentNode componentList = baseNode.GetComponentNode(27);

        for (int i = 0; i < idArray.Length; i++) {

            uint id = idArray[i];

            ComponentNode listRendererNode = componentList.GetComponentNode(id);

            AtkTextNode* textNode = listRendererNode.GetNode<AtkTextNode>(6);
            if (textNode == null) continue;

            AtkTextNode* numberNode = listRendererNode.GetNode<AtkTextNode>(5);
            if (numberNode == null) continue;

            AtkTextNode* tNode = listRendererNode.GetNode<AtkTextNode>(customDateTextNodeID);
            if (tNode == null)
            {
                tNode = CreateTextNode(customDateTextNodeID);
                if (tNode == null) continue;

                MergeTextBetweenElements(tNode, &numberNode->AtkResNode, &textNode->AtkResNode, &listRendererNode.GetPointer()->Component->UldManager);

                tNode->SetXFloat(textNode->X);
                tNode->Y = -7;

                tNode->TextColor = textNode->TextColor;
                tNode->EdgeColor = textNode->EdgeColor;
                tNode->BackgroundColor = textNode->BackgroundColor;
            }            

            byte* fullStringPointer = textNode->NodeText.StringPtr;
            if (fullStringPointer == null) continue;

            string? songName = Marshal.PtrToStringUTF8((nint)fullStringPointer);
            if (songName.IsNullOrWhitespace()) continue;

            Orchestrion? orchestrion = Sheets.GetOrchestrionByName(songName);
            if (orchestrion == null) continue;

            uint orchestrionDate = orchestrion?.RowId ?? 0;

            if (lastOrchestrionDate[i] == orchestrionDate) continue;

            lastOrchestrionDate[i] = orchestrionDate;

            if (DrawDate(tNode, orchestrionDate))
            {
                textNode->SetYFloat(7);
                GiveTooltip(baseAddon, tNode, orchestrionDate);
            }
            else
            {
                textNode->SetYFloat(0);
                ClearOldTooldtips();
            }
        }
    }
}
