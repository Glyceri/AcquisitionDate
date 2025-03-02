using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.ImageDatabase.Interfaces;
using AcquisitionDate.Windows.Components.Image;
using AcquistionDate.PetNicknames.TranslatorSystem;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using AcquistionDate.PetNicknames.Windowing.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Utility;
using AcquisitionDate.Windows.Windows.ListTabs.Tabs;
using AcquisitionDate.Windows.Windows.ListTabs;
using AcquisitionDate.Services.Interfaces;
using AcquisitionDate.Services;

namespace AcquisitionDate.Windows.Windows;

internal unsafe class AcquisitionListWindow : AcquisitionWindow
{
    protected override Vector2 MinSize { get; } = new Vector2(400, 250);
    protected override Vector2 MaxSize { get; } = new Vector2(1600, 1500);
    protected override Vector2 DefaultSize { get; } = new Vector2(800, 500);

    protected override bool HasHeaderBar { get; } = true;

    readonly IUserList UserList;
    readonly IDatabase Database;
    readonly IImageDatabase ImageDatabase;
    readonly IAcquisitionServices AcquisitionServices;

    float BarHeight => 30 * ImGuiHelpers.GlobalScaleSafe;

    bool inUserMode = false;
    bool lastInUserMode = false;
    bool isLocalEntry = false;

    string SearchText = string.Empty;
    string activeSearchText = string.Empty;

    IDatableData? ActiveEntry;
    IDatableUser? lastUser;

    readonly DataTab AchievementTab;

    public AcquisitionListWindow(WindowHandler windowHandler, IAcquisitionServices services, IUserList userList, IDatabase database, IImageDatabase imageDatabase) : base(windowHandler, services.Configuration, "Acquisition List")
    {
        Open();

        UserList = userList;
        Database = database;
        ImageDatabase = imageDatabase;
        AcquisitionServices = services;

        AchievementTab = new AchievementTab(AcquisitionServices.Sheets);
    }

    public override void OnOpen()
    {
        ClearSearchBar();
        SetUser(UserList.ActiveUser?.Data);
    }

    protected override void OnDraw()
    {
        if (lastUser != UserList.ActiveUser)
        {
            lastUser = UserList.ActiveUser;
            SetUser(lastUser?.Data);
        }

        DrawHeader();
        DrawSearchbar();

        if (inUserMode)
        {
            DrawUserList();
        }
        else
        {
            DrawTabList();
        }
    }

    void DrawHeader()
    {
        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(250, 110) * ImGuiHelpers.GlobalScale))
        {
            PlayerImage.Draw(ActiveEntry, in ImageDatabase);
            ImGui.SameLine();

            if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", ImGui.GetContentRegionAvail()))
            {
                TextAligner.Align(TextAlignment.Left);

                float contentAvailableX = ImGui.GetContentRegionAvail().X;
                Vector2 barSize = new Vector2(contentAvailableX, BarHeight);

                if (ImGui.Button(ActiveEntry?.Name ?? Translator.GetLine("...") + $"##ToggleButtonButton_{WindowHandler.InternalCounter}", barSize))
                {
                    PluginHandlers.Framework.Run(() => ToggleUserMode());
                }
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    if (!inUserMode)
                    {
                        ImGui.SetTooltip(Translator.GetLine("PetList.UserList"));
                    }
                    else
                    {
                        ImGui.SetTooltip(Translator.GetLine("PetList.Title"));
                    }
                }
                BasicLabel.Draw(ActiveEntry?.HomeworldName ?? Translator.GetLine("..."), barSize);
                BasicLabel.Draw(ActiveEntry?.LodestoneID?.ToString() ?? Translator.GetLine("..."), barSize);

                TextAligner.PopAlignment();
                Listbox.End();
            }

            Listbox.End();
        }

        ImGui.SameLine();

        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 110 * ImGuiHelpers.GlobalScale)))
        {
            Listbox.End();
        }
    }

    void DrawSearchbar()
    {
        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 30 * ImGuiHelpers.GlobalScale)))
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            float buttSize = ImGui.GetContentRegionAvail().Y;

            bool clicked = false;

            if (ImGui.InputTextMultiline($"##InputText_{WindowHandler.InternalCounter}", ref SearchText, 64, new Vector2(ImGui.GetContentRegionAvail().X - buttSize - style.FramePadding.X, buttSize), ImGuiInputTextFlags.CtrlEnterForNewLine | ImGuiInputTextFlags.EnterReturnsTrue))
            {
                clicked |= true;
            }

            SearchText = SearchText.Replace("\n", string.Empty);

            ImGui.SameLine();

            ImGui.PushFont(UiBuilder.IconFont);

            if (ImGui.Button($"{FontAwesomeIcon.Search.ToIconString()}##Search_{WindowHandler.InternalCounter}", new Vector2(buttSize, buttSize)))
            {
                clicked |= true;
            }

            ImGui.PopFont();

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Translator.GetLine("Search"));
            }

            if (clicked)
            {
                activeSearchText = SearchText;
                PluginHandlers.Framework.Run(() => SetUser(ActiveEntry));
            }

            Listbox.End();
        }
    }

    void DrawUserList()
    {
        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", ImGui.GetContentRegionAvail()))
        {
            foreach (IDatableData data in Database.GetEntries())
            {
                if (!SearchText.IsNullOrWhitespace())
                {
                    bool valid =
                        data.Name           .Contains(SearchText) ||
                        data.HomeworldName  .Contains(SearchText);
                    
                    if (!valid) continue;
                }

                if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 110 * ImGuiHelpers.GlobalScale)))
                {
                    PlayerImage.Draw(data, in ImageDatabase);
                    ImGui.SameLine();

                    if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", ImGui.GetContentRegionAvail()))
                    {
                        if (LabledLabel.DrawButton("Username:", data.Name, new Vector2(ImGui.GetContentRegionAvail().X, BarHeight)))
                        {
                            PluginHandlers.Framework.Run(() =>
                            {
                                ToggleUserMode();
                                SetUser(data);
                            });
                        }
                        LabledLabel.Draw("Homeworld:", data.HomeworldName, new Vector2(ImGui.GetContentRegionAvail().X, BarHeight));
                        LabledLabel.Draw("Lodestone ID:", data.LodestoneID?.ToString() ?? Translator.GetLine("..."), new Vector2(ImGui.GetContentRegionAvail().X, BarHeight));

                        Listbox.End();
                    }
                    Listbox.End();
                }
            }

            Listbox.End();
        }
    }

    void DrawTabList()
    {
        if (ActiveEntry == null) return;

        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", ImGui.GetContentRegionAvail()))
        {
            AchievementTab.Draw(ActiveEntry.GetDate(AchievementTab.MyType), activeSearchText);
            Listbox.End();
        }
    }

    void ClearSearchBar()
    {
        SearchText = string.Empty;
        activeSearchText = string.Empty;
    }

    void ToggleUserMode()
    {
        inUserMode = !inUserMode;

        if (!inUserMode && UserList.ActiveUser != null)
        {
            ActiveEntry = UserList.ActiveUser?.Data;
        }

        SetUser(ActiveEntry);
    }

    public void SetUser(IDatableData? entry)
    {
        bool completeUserChange = ActiveEntry != entry;

        isLocalEntry = HandleIfLocalEntry(entry);

        ActiveEntry = entry;

        if (lastInUserMode != inUserMode || completeUserChange)
        {
            lastInUserMode = inUserMode;
            ClearSearchBar();
        }
    }

    bool HandleIfLocalEntry(IDatableData? entry)
    {
        if (UserList.ActiveUser != null && entry != null)
        {
            return UserList.ActiveUser.ContentID == entry.ContentID;
        }
        else
        {
            return false;
        }
    }

    protected override void OnDispose()
    {

    }
}
