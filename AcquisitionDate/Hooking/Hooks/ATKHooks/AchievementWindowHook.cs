using Acquisition.PetNicknames.Hooking;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AcquisitionDate.Hooking.Hooks.ATKHooks;

internal unsafe class AchievementWindowHook : DateTextHook
{
    readonly uint[] listRenderers = [4, 41001, 41002, 41003, 41004, 41005, 41006, 41007, 41008];
    readonly string[] achievementNames = new string[9]; // the 9 is there because list renderers has 9 entries c: c: c: c: c:
    const uint customDateTextNodeID = 80;

    public AchievementWindowHook(IUserList userList, ISheets sheets, Configuration configuration) : base(userList, sheets, configuration) { }

    public override void Init() =>  PluginHandlers.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "Achievement", HookDetour);

    protected override IDatableList GetList(IDatableData userData) => userData.AchievementList;

    protected override unsafe void OnHookDetour(BaseNode baseNode, ref AtkUnitBase* baseAddon)
    {
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
            if (tNode == null)
            {
                tNode = CreateTextNode(customDateTextNodeID);
                if (tNode == null) continue;

                MergeTextBetweenElements(tNode, &prevNode->AtkResNode, &titleNode->AtkResNode, &listRendererNode.GetPointer()->Component->UldManager);

                tNode->AtkResNode.SetXFloat(titleNode->GetXFloat());
                tNode->AtkResNode.Y += 3;

                tNode->TextColor.R = 76;
                tNode->TextColor.G = 52;
                tNode->TextColor.B = 47;
                GiveTooltip(baseAddon, tNode, 0);
            }

            string achievementName = titleNode->NodeText.ToString();
            if (achievementNames[i] == achievementName) continue;

            achievementNames[i] = achievementName;

            Achievement? achievement = Sheets.GetAchievement(achievementName);
            if (achievement == null)
            {
                tNode->SetText(string.Empty);
                continue;
            }

            uint ID = achievement.Value.RowId;

            DrawDate(tNode, ID);
        }
    }

    protected override void OnDispose() { }
}
