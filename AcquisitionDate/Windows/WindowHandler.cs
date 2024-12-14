using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Windows.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Windows.Windows;
using AcquisitionDate.Database.Interfaces;

namespace AcquisitionDate.Windows;

internal class WindowHandler : IDisposable
{
    static int _internalCounter = 0;
    public static int InternalCounter { get => _internalCounter++; }

    readonly WindowSystem WindowSystem;

    readonly IUserList UserList;
    readonly IDatabase Database;

    public WindowHandler(IDalamudPluginInterface pluginInterface, IUserList userList, IDatabase database)
    {
        WindowSystem = new WindowSystem("AcquisitionDate");
        pluginInterface.UiBuilder.Draw += Draw;

        UserList = userList;
        Database = database;

        Register();
    }

    void Register()
    {
        AddWindow(new AcquisitionDebugWindow(UserList, Database));
    }

    void Draw()
    {
        _internalCounter = 0;
        WindowSystem.Draw();
    }

    public void AddWindow(AcquisitionWindow acquisitionWindow)
    {
        WindowSystem.AddWindow(acquisitionWindow);
    }

    public void RemoveWindow(AcquisitionWindow acquisitionWindow)
    {
        WindowSystem.RemoveWindow(acquisitionWindow);
    }

    public void OpenWindow<T>() where T : AcquisitionWindow
    {
        foreach(Window w in WindowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = true;
        }
    }

    public void CloseWindow<T>() where T : AcquisitionWindow
    {
        foreach (Window w in WindowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = false;
        }
    }

    public void ToggleWindow<T>() where T : AcquisitionWindow
    {
        foreach (Window w in WindowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen ^= true;
        }
    }

    public void Dispose()
    {
        PluginHandlers.PluginInterface.UiBuilder.Draw -= Draw;
        ClearAllWindows();
    }

    void ClearAllWindows()
    {
        foreach (IAcquisitionWindow window in WindowSystem.Windows)
        {
            window?.Dispose();
        }

        WindowSystem?.RemoveAllWindows();
    }
}
