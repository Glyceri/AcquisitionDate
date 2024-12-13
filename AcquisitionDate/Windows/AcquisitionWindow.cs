using Dalamud.Interface.Windowing;
using ImGuiNET;
using AcquisitionDate.Core.Handlers;
using AcquisitionDate.Windows.Attributes;
using AcquisitionDate.Windows.Interfaces;

namespace AcquisitionDate.Windows;

internal abstract class AcquisitionWindow : Window, IAcquisitionWindow
{
    public string Name { get; private set; } = "";

    public AcquisitionWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse, bool forceMainWindow = false) : base(name, flags, forceMainWindow) 
    {
        Name = name;
        PluginHandlers.Plugin.WindowHandler.AddWindow(this);
        HandleAttributing();
    }

    void HandleAttributing()
    {
        object[] attributes = GetType().GetCustomAttributes(true);
        foreach(object attribute in attributes)
        {
            if (attribute is SettingsWindowAttribute) PluginHandlers.PluginInterface.UiBuilder.OpenConfigUi += () => IsOpen = true;
            if (attribute is MainWindowAttribute) PluginHandlers.PluginInterface.UiBuilder.OpenMainUi += () => IsOpen = true;
        }
    }
}
