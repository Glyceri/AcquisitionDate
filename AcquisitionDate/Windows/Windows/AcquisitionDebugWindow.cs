using AcquisitionDate.Acquisition.Elements;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Interface.Utility;
using ImGuiNET;
using PetRenamer.PetNicknames.TranslatorSystem;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AcquisitionDate.Windows.Windows;

internal class AcquisitionDebugWindow : AcquisitionWindow
{
    readonly Configuration Configuration;
    readonly IUserList UserList;
    readonly IDatabase Database;
    readonly IAcquisitionServices Services;

    protected override Vector2 MinSize { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize { get; } = new Vector2(800, 400);

    int currentActive = 0;
    List<DevStruct> devStructList = new List<DevStruct>();

    public AcquisitionDebugWindow(IAcquisitionServices services, IUserList userList, IDatabase database) : base("Acquisition Dev Window")
    {
        Services = services;
        UserList = userList;
        Database = database;
        Configuration = services.Configuration;

        devStructList.Add(new DevStruct("User List", DrawUserList));
        devStructList.Add(new DevStruct("User Database", DrawUserDatabase));

        Open();
    }

    int current = 1480;
    bool stopPrint = false;

    void DrawMenu(string title, string[] elements, ref int configurationInt, float width = 0)
    {
        if (configurationInt < 0 || configurationInt >= elements.Length)
        {
            configurationInt = 0;
        }

        if (width <= 0) width = ImGui.GetContentRegionAvail().X;

        ImGui.SetNextItemWidth(width);

        if (ImGui.BeginCombo(title, elements[configurationInt], ImGuiComboFlags.PopupAlignLeft))
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (ImGui.Selectable(elements[i], i == configurationInt, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    configurationInt = i;
                    Configuration.Save();
                }
            }

            ImGui.EndCombo();
        }
    }

    unsafe void DrawUserList()
    {
        Vector2 size = new Vector2(ImGui.GetContentRegionAvail().X, 30 * ImGuiHelpers.GlobalScale);

        DrawMenu(Translator.GetLine("DateFormat"), Configuration.DateFormatString, ref Configuration.DateType, 120);
        DrawMenu(Translator.GetLine("PluginLanguage"), Configuration.Languages, ref Configuration.AcquisitionLanuage, 120);

        ImGui.NewLine();

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null)
        {
            ImGui.LabelText("", "No Local User Found");
        }
        else
        {
            ImGui.LabelText(localUser.Name, "Name: ");
            ImGui.LabelText(localUser.Homeworld.ToString(), "Homeworld: ");
            ImGui.LabelText(localUser.Data.HomeworldName.ToString(), "Homeworld Name: ");
            ImGui.LabelText(localUser.ContentID.ToString(), "ContentID: ");
            ImGui.LabelText(localUser.LodestoneID?.ToString() ?? "...", "Lodestone ID: ");

            ImGui.NewLine();

            

            ImGui.BeginDisabled(localUser.LodestoneID == null);

            if (ImGui.Button("Aqcuire Achievements"))
            {
                AchievementAcquirer.Instance.Acquire(localUser.Data);
            }

            ImGui.EndDisabled();

            ImGui.BeginDisabled(!AchievementAcquirer.Instance.IsAcquiring);

            if (ImGui.Button("Cancel Acquirement"))
            {
                AchievementAcquirer.Instance.Cancel();
            }

            ImGui.EndDisabled();

            ImGui.LabelText(AchievementAcquirer.Instance.IsAcquiring.ToString(), "Is Acquiring: ");
            ImGui.LabelText(AchievementAcquirer.Instance.CompletionRate.ToString(), "Percentage Complete: ");

            ImGui.NewLine();


            ImGui.BeginDisabled(localUser.LodestoneID == null);

            if (ImGui.Button("Aqcuire Quests"))
            {
                QuestAcquirer.Instance.Acquire(localUser.Data);
            }

            ImGui.EndDisabled();

            ImGui.BeginDisabled(!QuestAcquirer.Instance.IsAcquiring);

            if (ImGui.Button("Cancel Acquirement 2"))
            {
                QuestAcquirer.Instance.Cancel();
            }

            ImGui.EndDisabled();

            ImGui.LabelText(QuestAcquirer.Instance.IsAcquiring.ToString(), "Is Acquiring: ");
            ImGui.LabelText(QuestAcquirer.Instance.CompletionRate.ToString(), "Percentage Complete: ");

            ImGui.NewLine();


            ImGui.BeginDisabled(localUser.LodestoneID == null);

            if (ImGui.Button("Aqcuire Minios"))
            {
                MinionAcquirer.Instance.Acquire(localUser.Data);
            }

            ImGui.EndDisabled();

            ImGui.BeginDisabled(!MinionAcquirer.Instance.IsAcquiring);

            if (ImGui.Button("Cancel Acquirement 3"))
            {
                MinionAcquirer.Instance.Cancel();
            }

            ImGui.EndDisabled();

            ImGui.LabelText(MinionAcquirer.Instance.IsAcquiring.ToString(), "Is Acquiring: ");
            ImGui.LabelText(MinionAcquirer.Instance.CompletionRate.ToString(), "Percentage Complete: ");

            ImGui.NewLine();


            ImGui.BeginDisabled(localUser.LodestoneID == null);

            if (ImGui.Button("Aqcuire Mounts"))
            {
                MountAcquirer.Instance.Acquire(localUser.Data);
            }

            ImGui.EndDisabled();

            ImGui.BeginDisabled(!MountAcquirer.Instance.IsAcquiring);

            if (ImGui.Button("Cancel Acquirement 4"))
            {
                MountAcquirer.Instance.Cancel();
            }

            ImGui.EndDisabled();

            ImGui.LabelText(MountAcquirer.Instance.IsAcquiring.ToString(), "Is Acquiring: ");
            ImGui.LabelText(MountAcquirer.Instance.CompletionRate.ToString(), "Percentage Complete: ");

            ImGui.NewLine();


            ImGui.BeginDisabled(localUser.LodestoneID == null);

            if (ImGui.Button("Aqcuire Facewear"))
            {
                FacewearAcquirer.Instance.Acquire(localUser.Data);
            }

            ImGui.EndDisabled();

            ImGui.BeginDisabled(!FacewearAcquirer.Instance.IsAcquiring);

            if (ImGui.Button("Cancel Acquirement 5"))
            {
                FacewearAcquirer.Instance.Cancel();
            }

            ImGui.EndDisabled();

            ImGui.LabelText(FacewearAcquirer.Instance.IsAcquiring.ToString(), "Is Acquiring: ");
            ImGui.LabelText(FacewearAcquirer.Instance.CompletionRate.ToString(), "Percentage Complete: ");
        }
    }

    void DrawUserDatabase()
    {

    }

    public override void OnOpen()
    {
        if (devStructList.Count <= currentActive) return;

        devStructList[currentActive].requestUpdate?.Invoke(true);
    }

    public override void OnClose()
    {
        for (int i = 0; i < devStructList.Count; i++)
        {
            devStructList[i].requestUpdate?.Invoke(false);
        }
    }

    protected override void OnDraw()
    {
        if (devStructList.Count == 0) return;

        ImGui.BeginTabBar("##DevTabBar");

        for (int i = 0; i < devStructList.Count; i++)
        {
            if (!ImGui.TabItemButton(devStructList[i].title)) continue;
            int lastActive = currentActive;
            if (lastActive == i) continue;
            currentActive = i;
            devStructList[lastActive].requestUpdate?.Invoke(false);
            devStructList[currentActive].requestUpdate?.Invoke(true);
        }

        devStructList[currentActive].onSelected?.Invoke();

        ImGui.EndTabBar();
    }

    struct DevStruct
    {
        public readonly string title;
        public readonly System.Action onSelected;
        public readonly Action<bool>? requestUpdate;

        public DevStruct(string title, System.Action onSelected, Action<bool>? requestUpdate = null)
        {
            this.title = title;
            this.onSelected = onSelected;
            this.requestUpdate = requestUpdate;
        }
    }
}
