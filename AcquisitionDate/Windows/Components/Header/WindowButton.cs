using AcquisitionDate;
using AcquisitionDate.Windows;
using Dalamud.Interface;
using ImGuiNET;
using System.Numerics;

namespace AcquistionDate.PetNicknames.Windowing.Components.Header;

internal static class WindowButton
{
    public static float Width => GetWidth();

    public static void Draw<T>(WindowHandler handler, Configuration configuration, FontAwesomeIcon icon, string tooltip) where T : AcquisitionWindow
    { 
        T? window = handler.GetWindow<T>();
        if (window == null) return;

        bool isActive = window.IsOpen;

        ImGui.BeginDisabled(isActive && !configuration.quickButtonsToggle);

        float size = Width;

        ImGui.PushFont(UiBuilder.IconFont);

        // Hehe sex
        bool shouldDoWindow = ImGui.Button($"{icon.ToIconString()}##quickButton_{WindowHandler.InternalCounter}", new Vector2(size, size));


        ImGui.PopFont();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        ImGui.EndDisabled();

        if (!shouldDoWindow) return;

        if (configuration.quickButtonsToggle)
        {
            window.Toggle();
        }
        else
        {
            window.Open();
        }
    }

    static float GetWidth()
    {
        float height = ImGui.GetContentRegionAvail().Y;

        return height;
    }
}
