using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Windows.Interfaces;
using AcquisitionDate.DatableUsers.Interfaces;
using AcquisitionDate.Windows.Windows;
using AcquisitionDate.Database.Interfaces;
using AcquisitionDate.Services.Interfaces;
using AcquistionDate.PetNicknames.Windowing.Components;
using AcquisitionDate.PetNicknames.Windowing.Windows;

namespace AcquisitionDate.Windows;

internal class WindowHandler : IDisposable
{
    static int _internalCounter = 0;
    public static int InternalCounter { get => _internalCounter++; }

    readonly WindowSystem WindowSystem;

    readonly IAcquisitionServices Services;
    readonly IUserList UserList;
    readonly IDatabase Database;

    public WindowHandler(IAcquisitionServices services, IUserList userList, IDatabase database)
    {
        WindowSystem = new WindowSystem("AcquisitionDate");
        PluginHandlers.PluginInterface.UiBuilder.Draw += Draw;
        PluginHandlers.PluginInterface.UiBuilder.OpenConfigUi += OpenWindow<AcquisitionConfigWindow>;

        Services = services;
        UserList = userList;
        Database = database;

        ComponentLibrary.Initialise();

        Register();
    }

    void Register()
    {
        AddWindow(new AcquisitionDebugWindow(Services, UserList, Database, this, Services.Configuration));
        AddWindow(new AcquisitionConfigWindow(this, Services.Configuration));
        AddWindow(new KofiWindow(this, Services.Configuration));
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

    public T? GetWindow<T>() where T : AcquisitionWindow
    {
        foreach (AcquisitionWindow window in WindowSystem.Windows)
        {
            if (window is not T acquisitionWindow) continue;

            return acquisitionWindow;
        }

        return null;
    }

    public void Dispose()
    {
        PluginHandlers.PluginInterface.UiBuilder.Draw -= Draw;
        PluginHandlers.PluginInterface.UiBuilder.OpenConfigUi -= OpenWindow<AcquisitionConfigWindow>;

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
