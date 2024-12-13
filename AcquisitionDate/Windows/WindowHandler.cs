using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using AcquisitionDate.Core.Handlers;

namespace AcquisitionDate.Windows;

internal class WindowHandler : IDisposable
{
    WindowSystem windowSystem;

    public WindowHandler(IDalamudPluginInterface pluginInterface)
    {
        windowSystem = new WindowSystem("AcquisitionDate");
        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
    }

    public void Dispose()
    {
        PluginHandlers.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        windowSystem.RemoveAllWindows();
    }

    public void AddWindow(AcquisitionWindow acquisitionWindow)
    {
        windowSystem.AddWindow(acquisitionWindow);
    }

    public void RemoveWindow(AcquisitionWindow acquisitionWindow)
    {
        windowSystem.RemoveWindow(acquisitionWindow);
    }

    public void OpenWindow<T>() where T : AcquisitionWindow
    {
        foreach(Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = true;
        }
    }

    public void CloseWindow<T>() where T : AcquisitionWindow
    {
        foreach (Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen = false;
        }
    }

    public void ToggleWindow<T>() where T : AcquisitionWindow
    {
        foreach (Window w in windowSystem.Windows)
        {
            if (w is not T) continue;
            w.IsOpen ^= true;
        }
    }
}
