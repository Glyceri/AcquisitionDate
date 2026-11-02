using AcquisitionDate.Database.Enums;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.LodestoneData;
using AcquisitionDate.LodestoneNetworking.Enums;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.LodestoneNetworking.Structs;
using AcquisitionDate.Parser.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using Dalamud.Interface.Utility;
using HtmlAgilityPack;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;
using AcquisitionDate.Database.Structs;
using AcquisitionDate.Translation;

namespace AcquisitionDate.Windows.Windows;

internal class AcquisitionDebugWindow : AcquisitionWindow
{
    readonly IDatabase Database;
    readonly IAcquisitionServices Services;
    readonly IAcquisitionParser Parser;
    readonly ILodestoneNetworker LodestoneNetworker;

    protected override Vector2 MinSize { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize { get; } = new Vector2(800, 400);
    protected override bool HasHeaderBar { get; } = true;

    int currentActive = 0;
    List<DevStruct> devStructList = new List<DevStruct>();

    public AcquisitionDebugWindow(IAcquisitionServices services, IDatabase database, WindowHandler windowHandler, Configuration configuration, IAcquisitionParser parser, ILodestoneNetworker lodestoneNetworker) : base(windowHandler, configuration,"Acquisition Dev Window")
    {
        Services = services;
        Database = database;
        Parser = parser;
        LodestoneNetworker = lodestoneNetworker;

        devStructList.Add(new DevStruct("User List", DrawUserList));
        devStructList.Add(new DevStruct("User Database", DrawUserDatabase));
        devStructList.Add(new DevStruct("Parsers", DrawParserTab));
        devStructList.Add(new DevStruct("Networking Queue", DrawNetworkingTab));

        if (configuration.debugModeActive && configuration.openDebugWindowOnStart)
        {
            Open();
        }
    }

    void DrawNetworkingTab()
    {
        Vector2 labelSize = new Vector2(ImGui.GetContentRegionAvail().X, 35 * ImGuiHelpers.GlobalScale);

        foreach (LodestoneQueueElementData data in LodestoneNetworker.GetAllQueueData())
        {
            LabledLabel.Draw("Url: ", data.URL, labelSize);
            LabledLabel.Draw("Queue State: ", data.QueueState.ToString(),  labelSize);
            LabledLabel.Draw("Time In Queue: ", data.TimeInQueue.ToString(),  labelSize);
            LabledLabel.Draw("Tick Count: ", data.TickCount.ToString(),  labelSize);
            LabledLabel.Draw("At Tick: ", data.AtTick.ToString(),  labelSize);
            LabledLabel.Draw("Disposed: ", data.Disposed.ToString(),  labelSize);

            ImGui.NewLine();
        }
    }

    string parseText = string.Empty;
    LodestoneRegion currentRegion = LodestoneRegion.Europe;

    void DrawParserTab()
    {
        if (ImGui.BeginMenu($"Lodestone Region ({currentRegion})##lodestoneRegionMenu"))
        {
            if (ImGui.MenuItem("Europe")) currentRegion = LodestoneRegion.Europe;
            if (ImGui.MenuItem("America")) currentRegion = LodestoneRegion.America;
            if (ImGui.MenuItem("Germany")) currentRegion = LodestoneRegion.Germany;
            if (ImGui.MenuItem("France")) currentRegion = LodestoneRegion.France;
            if (ImGui.MenuItem("Japan")) currentRegion = LodestoneRegion.Japan;

            ImGui.EndMenu();
        }

        ImGui.InputTextMultiline("##parserText", ref parseText, 1000000, ImGui.GetContentRegionAvail() * new Vector2(1, 0.5f));

        if (ImGui.Button($"Parse##parser{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 35)))
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(parseText);
            PluginHandlers.PluginLog.Verbose($"Try Parse:\n{document.DocumentNode.InnerHtml}");
            Parser.ItemPageDataParser.SetPageLanguage(currentRegion);
            Parser.ItemPageDataParser.SetGetIDFunc(GetID);
            Parser.ItemPageDataParser.SetListIconName("minion");
            Parser.ItemPageDataParser.Parse(document.DocumentNode, OnSuccess, OnFailure);
        }
    }

    uint? GetID(string name)
    {
        return Services.Sheets.GetCompanionByName(name)?.ID ?? null;
    }

    void OnSuccess(ItemData itemData)
    {
        PluginHandlers.PluginLog.Verbose($"New Item Data: {itemData.ItemID}, {itemData.AchievedDate}");
    }

    void OnFailure(Exception e)
    {
        PluginHandlers.PluginLog.Error(e, "Error in parse!");
    }

    void DrawUserList()
    {
        ulong activeContentId = PluginHandlers.PlayerState.ContentId;
        string name = PluginHandlers.PlayerState.CharacterName;
        uint homeworld = PluginHandlers.PlayerState.HomeWorld.RowId;
        bool isLoaded = PluginHandlers.PlayerState.IsLoaded;
        
        ImGui.Text($"Is Loaded: {isLoaded}, Active ContentId: {activeContentId}, Name: {name}, Homeworld: {homeworld}");
        
        IDatableData? data = Database.GetLocalUser();
        
        if (data == null)
        {
            ImGui.LabelText("", "No Local User Found");
        }
        else
        {
            DrawUser(data);
        }
    }

    void DrawUser(IDatableData user)
    {
        ImGui.LabelText(user.Name, "Name: ");
        ImGui.LabelText(user.Homeworld.ToString(), "Homeworld: ");
        ImGui.LabelText(user.HomeworldName.ToString(), "Homeworld Name: ");
        ImGui.LabelText(user.ContentId.ToString(), "ContentID: ");
        ImGui.LabelText(user.LodestoneId?.ToString() ?? "...", "Lodestone ID: ");
    }

    void DrawUserDatabase()
    {
        IDatableData[] entries = Database.GetEntries();

        for (int i = 0; i < entries.Length; i++)
        {
            IDatableData entry = entries[i];

            if (ImGui.CollapsingHeader($"{entry.Name} {entry.HomeworldName}"))
            {
                if (ImGui.Button("SAVE"))
                {
                    Services.Configuration.Save();
                }
                DrawUser(entry);
                ImGui.Indent();
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Achievement), "Achievements", (id) => Services.Sheets.GetAchievementByID(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Quest), "Quests", (id) => Services.Sheets.GetQuest(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Minion), "Minions", (id) => Services.Sheets.GetCompanion((ushort)id)?.BaseSingular ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Mount), "Mounts", (id) => Services.Sheets.GetMountByID(id)?.Singular.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.ClassLVL), "Levels", (id) => $"{id}");

                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Facewear), "Facewear", (id) => Services.Sheets.GetGlassesById(id)?.Singular.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Orchestrion), "Orchestrion Roll", (id) => Services.Sheets.GetOrchestrionByID(id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Card), "Cards", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Fashion), "Fashion", (id) => Services.Sheets.GetOrnamentById(id)?.Singular.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Duty), "Duties", (id) => Services.Sheets.GetContentFinderCondition((ushort)id)?.Name.ExtractText() ?? "[Unknown]");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Fishing), "Fishing", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Incognita), "Sightseeing List", (id) => Services.Sheets.GetAdventureByIndex(id)?.Name.ExtractText() ?? "[Unknown]");

                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.Framers), "Framing Kits", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.SecretRecipeBook), "Secret Recipe Book", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.BuddyEquip), "Bardings", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.UnlockLink), "Unlock Link???? (idk what this is)", (id) => $"{id}");
                DrawDatableList(entry, entry.GetDateList(AcquirableDateType.FolkloreTome), "Folklores", (id) => $"{id}");
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

                    uint elementID = list.GetId(i);
                    UnlockedDate? lodestoneDate = list.GetDate(elementID);
                    UnlockedDate? manualDate = list.GetDate(elementID);

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
                    ImGui.Text($"{lodestoneDate?.GetSpecificDateTime(AcquiredDateType.Lodestone)?.GetTimeString() ?? "[No Date Found]"}");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{manualDate?.GetSpecificDateTime(AcquiredDateType.Manual)?.GetTimeString() ?? "[No Date Found]"}");
                }

                ImGui.EndTable();
            }
            ImGui.Unindent();
        }
    }

    public override void OnOpen()
    {
        currentActive = Configuration.lastActiveDevTab;
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
            if (!ImGui.TabItemButton(devStructList[i].title, i == currentActive ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None)) continue;
            int lastActive = currentActive;
            if (lastActive == i) continue;
            currentActive = i;
            devStructList[lastActive].requestUpdate?.Invoke(false);
            devStructList[currentActive].requestUpdate?.Invoke(true);
            Configuration.lastActiveDevTab = currentActive;
            Configuration.Save();
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
