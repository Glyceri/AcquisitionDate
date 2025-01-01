using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquistionDate.PetNicknames.TranslatorSystem;
using AcquistionDate.PetNicknames.Windowing.Components;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AcquisitionDate.Windows.Windows;

internal class AcquisitionDebugWindow : AcquisitionWindow
{
    readonly IUserList UserList;
    readonly IDatabase Database;
    readonly IAcquisitionServices Services;

    protected override Vector2 MinSize { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize { get; } = new Vector2(800, 400);
    protected override bool HasHeaderBar { get; } = true;

    int currentActive = 0;
    List<DevStruct> devStructList = new List<DevStruct>();

    public AcquisitionDebugWindow(IAcquisitionServices services, IUserList userList, IDatabase database, WindowHandler windowHandler, Configuration configuration) : base(windowHandler, configuration,"Acquisition Dev Window")
    {
        Services = services;
        UserList = userList;
        Database = database;

        devStructList.Add(new DevStruct("User List", DrawUserList));
        devStructList.Add(new DevStruct("User Database", DrawUserDatabase));

        if (configuration.debugModeActive && configuration.openDebugWindowOnStart)
        {
            Open();
        }
    }

    unsafe void DrawUserList()
    {
        Vector2 size = new Vector2(ImGui.GetContentRegionAvail().X, 30 * ImGuiHelpers.GlobalScale);

        IDatableUser? localUser = UserList.ActiveUser;
        if (localUser == null)
        {
            ImGui.LabelText("", "No Local User Found");
        }
        else
        {
            DrawUser(localUser.Data);
        }
    }

    void DrawUser(IDatableData user)
    {
        ImGui.LabelText(user.Name, "Name: ");
        ImGui.LabelText(user.Homeworld.ToString(), "Homeworld: ");
        ImGui.LabelText(user.HomeworldName.ToString(), "Homeworld Name: ");
        ImGui.LabelText(user.ContentID.ToString(), "ContentID: ");
        ImGui.LabelText(user.LodestoneID?.ToString() ?? "...", "Lodestone ID: ");
    }

    void DrawUserDatabase()
    {
        IDatableData[] entries = Database.GetEntries();

        for (int i = 0; i < entries.Length; i++)
        {
            IDatableData entry = entries[i];

            if (ImGui.CollapsingHeader($"{entry.Name} {entry.HomeworldName}"))
            {
                DrawUser(entry);
                ImGui.Indent();
                DrawDatableList(entry, entry.AchievementList, "Achievements", (id) => Services.Sheets.GetAchievementByID(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.QuestList, "Quests", (id) => Services.Sheets.GetQuest(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.MinionList, "Minions", (id) => Services.Sheets.GetCompanion((ushort)id)?.BaseSingular ?? "[Unknown]");
                DrawDatableList(entry, entry.MountList, "Mounts", (id) => Services.Sheets.GetMountByID(id)?.Singular.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.ClassLVLList, "Levels", (id) => $"{id}");

                DrawDatableList(entry, entry.FacewearList, "Facewear", (id) => Services.Sheets.GetGlassesByID(id)?.BaseSingular ?? "[Unknown]");
                DrawDatableList(entry, entry.OrchestrionList, "Orchestrion Roll", (id) => Services.Sheets.GetOrchestrionByID(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.CardList, "Cards", (id) => $"{id}");
                DrawDatableList(entry, entry.FashionList, "Fashion", (id) => Services.Sheets.GetOrnamentByID(id)?.Singular.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.DutyList, "Duties", (id) => Services.Sheets.GetContentFinderCondition((ushort)id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.CardList, "Fishing", (id) => $"{id}");
                DrawDatableList(entry, entry.SightList, "Sightseeing List", (id) => Services.Sheets.GetAdventureByIndex(id)?.Name.ExtractText() ?? "[Unknown]");

                DrawDatableList(entry, entry.FramersList, "Framing Kits", (id) => $"{id}");
                DrawDatableList(entry, entry.SecretRecipeBookList, "Secret Recipe Book", (id) => $"{id}");
                DrawDatableList(entry, entry.BuddyEquipList, "Bardings", (id) => $"{id}");
                DrawDatableList(entry, entry.UnlockLinkList, "Unlock Link???? (idk what this is)", (id) => $"{id}");
                DrawDatableList(entry, entry.FolkloreTomeList, "Folklores", (id) => $"{id}");
                ImGui.Unindent();
            }
        }
    }

    void DrawDatableList(IDatableData data, IDatableList list, string title, Func<uint, string> getElementName)
    {
        int listLength = list.Length;

        if (ImGui.CollapsingHeader($"{title} ({listLength})##{data.Name}_{data.HomeworldName}_{title}", ImGuiTreeNodeFlags.Framed))
        {
            ImGui.Indent();
            if (ImGui.BeginTable($"##{data.Name}_{data.HomeworldName}_{title}", 3))
            {
                ImGui.TableSetupColumn($"{title} Name", ImGuiTableColumnFlags.WidthFixed, 400 * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("Lodestone Date", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Manual Date", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                for (int i = 0; i < listLength; i++)
                {
                    ImGui.TableNextRow();
                    bool visible = ImGui.IsItemVisible();
                    ImGui.TableSetColumnIndex(0);

                    uint elementID = list.GetID(i);
                    DateTime? lodestoneDate = list.GetDate(elementID, AcquiredDateType.Lodestone);
                    DateTime? manualDate = list.GetDate(elementID, AcquiredDateType.Manual);

                    if (EraserButton.Draw(new Vector2(25,20), Translator.GetLine("ClearButton.Label"), Translator.GetLine("Acquiry.Clear")))
                    {
                        if (list.RemoveDate(elementID))
                        {
                            break;
                        }
                    }

                    ImGui.SameLine();

                    if (visible)
                    {
                        ImGui.Text($"{getElementName?.Invoke(elementID)}");
                    }
                    else
                    {
                        ImGui.Text($"{elementID}");
                    }
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{lodestoneDate?.ToString(Services.Configuration.DateParseString()) ?? "[No Date Found]"}");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{manualDate?.ToString(Services.Configuration.DateParseString()) ?? "[No Date Found]"}");
                }

                ImGui.EndTable();
            }
            ImGui.Unindent();
        }
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
