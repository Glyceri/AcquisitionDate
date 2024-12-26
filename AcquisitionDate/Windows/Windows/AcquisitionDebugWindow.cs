using AcquisitionDate.Acquisition.Elements;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Services.Interfaces;
using Dalamud.Interface.Utility;
using ImGuiNET;
using AcquistionDate.PetNicknames.TranslatorSystem;
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
            ImGui.LabelText(localUser.Name, "Name: ");
            ImGui.LabelText(localUser.Homeworld.ToString(), "Homeworld: ");
            ImGui.LabelText(localUser.Data.HomeworldName.ToString(), "Homeworld Name: ");
            ImGui.LabelText(localUser.ContentID.ToString(), "ContentID: ");
            ImGui.LabelText(localUser.LodestoneID?.ToString() ?? "...", "Lodestone ID: ");
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
