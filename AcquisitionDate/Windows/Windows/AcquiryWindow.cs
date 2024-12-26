﻿using AcquisitionDate.Acquisition.Interfaces;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.DirtySystem.Interfaces;
using AcquisitionDate.LodestoneNetworking.Interfaces;
using AcquisitionDate.Windows.Components.Labels;
using AcquistionDate.PetNicknames.Windowing.Components;
using AcquistionDate.PetNicknames.Windowing.Components.Labels;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Numerics;

namespace AcquisitionDate.Windows.Windows;

internal class AcquiryWindow : AcquisitionWindow
{
    protected override Vector2 MinSize { get; } = new Vector2(400, 450);
    protected override Vector2 MaxSize { get; } = new Vector2(600, 740);
    protected override Vector2 DefaultSize { get; } = new Vector2(600, 500);
    protected override bool HasHeaderBar { get; } = true;

    readonly IUserList UserList;
    readonly IDatabase Database;
    readonly IAcquirerHandler AcquirerHandler;
    readonly ILodestoneNetworker LodestoneNetworker;
    readonly IDirtyListener DirtyListener;

    float BarSize => 30 * ImGuiHelpers.GlobalScale;

    string sessionTokenInput = string.Empty;
    string lastSessionTokenInput = string.Empty;

    IDatableData? currentActiveData = null;

    public AcquiryWindow(WindowHandler windowHandler, Configuration configuration, IUserList userList, IDatabase database, IAcquirerHandler acquirerHandler, ILodestoneNetworker lodestoneNetworker, IDirtyListener dirtyListener) : base(windowHandler, configuration, "Acquisition Window")
    {
        UserList = userList;
        Database = database;
        AcquirerHandler = acquirerHandler;
        LodestoneNetworker = lodestoneNetworker;
        DirtyListener = dirtyListener;

        DirtyListener.RegisterDirtyUser(OnReset);
    }

    protected override void OnDispose()
    {
        DirtyListener.UnregisterDirtyUser(OnReset);
    }

    void OnReset()
    {
        sessionTokenInput = string.Empty;
        lastSessionTokenInput = string.Empty;
        currentActiveData = null;
        OnDataSet();
    }

    void OnDataSet()
    {

    }

    protected override void OnDraw()
    {
        if (currentActiveData == null)
        {
            IDatableUser? localUser = UserList.ActiveUser;
            if (localUser != null)
            {
                currentActiveData = localUser.Data;
                OnDataSet();
            }
        }

        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 37 * ImGuiHelpers.GlobalScale)))
        {
            TextAligner.Align(TextAlignment.Left);
            BasicLabel.Draw("Acquired date will automatically track the date of every upcoming event:", new Vector2(ImGui.GetContentRegionAvail().X - BarSize - ImGui.GetStyle().ItemSpacing.X, BarSize));
            TextAligner.PopAlignment();

            ImGui.SameLine();

            if (Listbox.Begin($"##HelpLabel{WindowHandler.InternalCounter}", new Vector2(BarSize, BarSize)))
            {
                ImGuiComponents.HelpMarker
                (
                    $"• Achievements\n" +
                    $"• Quests\n" +
                    $"• Duty Completion\n" +
                    $"• Character Levelup\n" +
                    $"• Fish\n" +
                    $"• Sightseeing Log\n" +
                    $"• Glasses\n" +
                    $"• Minions\n" +
                    $"• Mounts\n" +
                    $"• Orchestrion Rolls\n" +
                    $"• Fashion Accessories\n" +
                    $"• Other Item Unlocks",
                    FontAwesomeIcon.InfoCircle
                );
                Listbox.End();
            }

            Listbox.End();
        }

        ImGui.NewLine();

        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", new Vector2(ImGui.GetContentRegionAvail().X, 262 * ImGuiHelpers.GlobalScale)))
        {
            TextAligner.Align(TextAlignment.Left);
            BasicLabel.Draw("Some dates can be obtained retroactively from the Lodestone.", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            BasicLabel.Draw("This sadly requires a Lodestone session token.", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            if (LabledLabel.DrawButton("Session Token Tutorial: ##SessionTokenTutButton", "Click Here", new Vector2(ImGui.GetContentRegionAvail().X, BarSize)))
            {
                WindowHandler.OpenWindow<SessionTokenWindow>();
            }
            TextAligner.PopAlignment();

            if (RenameLabel.Draw("Session Token: ", sessionTokenInput == lastSessionTokenInput, ref sessionTokenInput, new Vector2(ImGui.GetContentRegionAvail().X, BarSize)))
            {
                lastSessionTokenInput = sessionTokenInput;
                LodestoneNetworker.SetSessionToken(sessionTokenInput);
            }

            ImGui.NewLine();

            TextAligner.Align(TextAlignment.Centre);
            BasicLabel.Draw("!!!   NEVER SHARE YOUR SESSION TOKEN WITH ANYONE   !!!", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            BasicLabel.Draw("!!!   (YES THIS TECHNICALLY INCLUDES ME AND THIS PLUGIN)   !!!", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            BasicLabel.Draw("!!!   ALWAYS LOG OUT OF LODESTONE WHEN YOU ARE DONE   !!!", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            TextAligner.PopAlignment();

            Listbox.End();
        }

        ImGui.NewLine();

        if (currentActiveData == null)
        {
            BasicLabel.Draw("Log in on a character to acquire their data.", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            return;
        }

        if (Listbox.Begin($"##Listbox_{WindowHandler.InternalCounter}", ImGui.GetContentRegionAvail()))
        {
            AcquirerUI.Draw(AcquirerHandler.AchievementAcquirer, currentActiveData, "Achievements", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            AcquirerUI.Draw(AcquirerHandler.QuestAcquirer, currentActiveData, "Quests", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            AcquirerUI.Draw(AcquirerHandler.MinionAcquirer, currentActiveData, "Minions", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            AcquirerUI.Draw(AcquirerHandler.MountAcquirer, currentActiveData, "Mounts", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            AcquirerUI.Draw(AcquirerHandler.FacewearAcquirer, currentActiveData, "Glasses", new Vector2(ImGui.GetContentRegionAvail().X, BarSize));
            Listbox.End();
        }
    }
}
